using System;
using System.Collections.Generic;
using System.ComponentModel;
using SLEP.WaveDisplay;
using System.Windows;
using System.Windows.Threading;
using NAudio.Wave;

namespace SLEP.Audio
{
	public class WaveFormPlayer : IWaveformPlayer, INotifyPropertyChanged, IDisposable
	{
		#region Fields
		private static WaveFormPlayer instance;
		private readonly DispatcherTimer positionTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);
		private readonly BackgroundWorker waveformGenerateWorker = new BackgroundWorker();
		private readonly int fftDataSize = (int)FFTDataSize.FFT2048;
		private bool disposed;
		private double channelLength;
		private double channelPosition;
		private bool inChannelSet;
		private WaveStream activeStream;
		private WaveChannel32 inputStream;
		private SampleAggregator sampleAggregator;
		private SampleAggregator waveformAggregator;
		private string pendingWaveformPath;
		private float[] fullLevelData;
		private float[] waveformData;
				
		#endregion

		#region Constants
		private const int waveformCompressedPointCount = 2000;
		private const int repeatThreshold = 200;
		#endregion

		#region Singleton Pattern
		public static WaveFormPlayer Instance
		{
			get
			{
				if (instance == null)
					instance = new WaveFormPlayer();
				return instance;
			}
		}
		#endregion

		#region Constructor
		private WaveFormPlayer()
		{
			waveformGenerateWorker.DoWork += waveformGenerateWorker_DoWork;
		}
		#endregion

		#region IDisposable
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		
		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					StopAndCloseStream();
				}

				disposed = true;
			}
		}
		#endregion


		#region IWaveformPlayer
		

		public TimeSpan SelectionBegin { get; set; }
		public TimeSpan SelectionEnd { get; set; }
	

		public float[] WaveformData
		{
			get { return waveformData; }
			protected set
			{
				float[] oldValue =  waveformData;
				waveformData = value;
				if (oldValue != waveformData)
					NotifyPropertyChanged("WaveformData");
			}
		}

		public double ChannelLength
		{
			get { return channelLength; }
			protected set
			{
				double oldValue = channelLength;
				channelLength = value;
				//if (oldValue != channelLength)
				//	NotifyPropertyChanged("ChannelLength");
			}
		}

		public double ChannelPosition
		{
			get { return channelPosition; }
			set
			{
				if (!inChannelSet)
				{
					
					inChannelSet = true; // Avoid recursion
					double oldValue = channelPosition;
					double position = Math.Max(0, Math.Min(value, ChannelLength));
					if (ActiveStream != null)
						ActiveStream.Position = (long)((position / ActiveStream.TotalTime.TotalSeconds) * ActiveStream.Length);
					channelPosition = position;
					if (oldValue != channelPosition)
						NotifyPropertyChanged("ChannelPosition");
					inChannelSet = false;
				}
			}
		}
		#endregion

		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;

		private void NotifyPropertyChanged(String info)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(info));
			}
		}
		#endregion

		#region Waveform Generation
		private class WaveformGenerationParams
		{
			public WaveformGenerationParams(int points, string path)
			{
				Points = points;
				Path = path;
			}

			public int Points { get; protected set; }
			public string Path { get; protected set; }
		}

		private void GenerateWaveformData(string path)
		{
			
			if (waveformGenerateWorker.IsBusy)
			{
				pendingWaveformPath = path;
				waveformGenerateWorker.CancelAsync();
				return;
			}
			

			if (!waveformGenerateWorker.IsBusy && waveformCompressedPointCount != 0)
				waveformGenerateWorker.RunWorkerAsync(new WaveformGenerationParams(waveformCompressedPointCount, path));
		}		

		private void waveformGenerateWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			var waveformParams = e.Argument as WaveformGenerationParams;
			var waveformWAVStream = new WaveFileReader(waveformParams.Path);
			var waveformInputStream = new WaveChannel32(waveformWAVStream);
			waveformInputStream.Sample += waveStream_Sample;

			int frameLength = fftDataSize;
			int frameCount = (int)((double)waveformInputStream.Length / (double)frameLength);
			int waveformLength = frameCount * 2;
			byte[] readBuffer = new byte[frameLength];
			waveformAggregator = new SampleAggregator(frameLength);

			float maxLeftPointLevel = float.MinValue;
			float maxRightPointLevel = float.MinValue;
			int currentPointIndex = 0;
			float[] waveformCompressedPoints = new float[waveformParams.Points];
			List<float> waveformData = new List<float>();
			List<int> waveMaxPointIndexes = new List<int>();

			for (int i = 1; i <= waveformParams.Points; i++)
			{
				waveMaxPointIndexes.Add((int)Math.Round(waveformLength * ((double)i / (double)waveformParams.Points), 0));
			}
			int readCount = 0;
			while (currentPointIndex * 2 < waveformParams.Points)
			{
				waveformInputStream.Read(readBuffer, 0, readBuffer.Length);

				waveformData.Add(waveformAggregator.LeftMaxVolume);
				waveformData.Add(waveformAggregator.RightMaxVolume);

				if (waveformAggregator.LeftMaxVolume > maxLeftPointLevel)
					maxLeftPointLevel = waveformAggregator.LeftMaxVolume;
				if (waveformAggregator.RightMaxVolume > maxRightPointLevel)
					maxRightPointLevel = waveformAggregator.RightMaxVolume;

				if (readCount > waveMaxPointIndexes[currentPointIndex])
				{
					waveformCompressedPoints[(currentPointIndex * 2)] = maxLeftPointLevel;
					waveformCompressedPoints[(currentPointIndex * 2) + 1] = maxRightPointLevel;
					maxLeftPointLevel = float.MinValue;
					maxRightPointLevel = float.MinValue;
					currentPointIndex++;
				}
				if (readCount % 3000 == 0)
				{
					float[] clonedData = (float[])waveformCompressedPoints.Clone();
				
					Application.Current.Dispatcher.Invoke(new Action(() =>
					{
						WaveformData = clonedData;
					}));
				}

				if (waveformGenerateWorker.CancellationPending)
				{
					e.Cancel = true;
					break;
				}
				readCount++;
			}

			float[] finalClonedData = (float[])waveformCompressedPoints.Clone();
			Application.Current.Dispatcher.Invoke(new Action(() =>
			{
				fullLevelData = waveformData.ToArray();
				WaveformData = finalClonedData;
			}));
			waveformInputStream.Close();
			waveformInputStream.Dispose();
			waveformInputStream = null;
			waveformWAVStream.Close();
			waveformWAVStream.Dispose();
			waveformWAVStream = null;
		}
		#endregion

		#region Private Utility Methods
		private void StopAndCloseStream()
		{
			
			if (activeStream != null)
			{
				inputStream.Close();
				inputStream = null;
				ActiveStream.Close();
				ActiveStream = null;
			}
			
		}
		#endregion

		#region Public Methods
		

		public void OpenFile(string path)
		{
		

			if (ActiveStream != null)
			{
				SelectionBegin = TimeSpan.Zero;
				SelectionEnd = TimeSpan.Zero;
				ChannelPosition = 0;
			}

			StopAndCloseStream();

			if (System.IO.File.Exists(path))
			{
				try
				{
					
					ActiveStream = new WaveFileReader(path);
					inputStream = new WaveChannel32(ActiveStream);
					sampleAggregator = new SampleAggregator(fftDataSize);
					inputStream.Sample += inputStream_Sample;
					
					ChannelLength = inputStream.TotalTime.TotalSeconds;
					GenerateWaveformData(path);
		
				}
				catch
				{
					ActiveStream = null;
		
				}
			}
		
		}
		#endregion

		#region Public Properties
		public WaveStream ActiveStream
		{
			get { return activeStream; }
			protected set
			{
				WaveStream oldValue = activeStream;
				activeStream = value;
				if (oldValue != activeStream)
					NotifyPropertyChanged("ActiveStream");
			}
		}
			   		 
		public bool IsPlaying
		{
			get { return false; }

		}
		#endregion

		#region Event Handlers
		private void inputStream_Sample(object sender, SampleEventArgs e)
		{
			sampleAggregator.Add(e.Left, e.Right);
			long repeatStartPosition = (long)((SelectionBegin.TotalSeconds / ActiveStream.TotalTime.TotalSeconds) * ActiveStream.Length);
			long repeatStopPosition = (long)((SelectionEnd.TotalSeconds / ActiveStream.TotalTime.TotalSeconds) * ActiveStream.Length);
			if (((SelectionEnd - SelectionBegin) >= TimeSpan.FromMilliseconds(repeatThreshold)) && ActiveStream.Position >= repeatStopPosition)
			{
				sampleAggregator.Clear();
				ActiveStream.Position = repeatStartPosition;
			}
		}

		void waveStream_Sample(object sender, SampleEventArgs e)
		{
			waveformAggregator.Add(e.Left, e.Right);
		}

	
	
		#endregion
	}
}
