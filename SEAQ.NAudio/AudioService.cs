using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using SLEP.NAudio.Enums;
using System.Windows;
using System.IO;
using SLEP.Logger;

namespace SLEP.Audio
{

	[Export(typeof(IAudioService))]
	public class AudioService : IAudioService
	{
		public event PlayStopHandle PlaybackStop;
		public delegate void PlayStopHandle(EventArgs e);
		private WaveOutEvent _device;
		private WaveStream _waveStream { get; set; }
		private IList<WaveStream> _waveStreamList;
		public string _audioFileName { get; set; }
		private MixingWaveProvider32 _mixer;

		[ImportingConstructor]
		public AudioService()
		{

		}

		public bool IntializeNAudioLibrary(int latency)
		{
			try
			{
				_device = new WaveOutEvent();
				_waveStreamList = new List<WaveStream>();

				_device.DesiredLatency = 100;//latency;//_device.DesiredLatency = 100;
				_device.NumberOfBuffers = 2 ;//100 / latency ;
				return true;
			}
			catch(Exception ex)
			{
				Log.Write(LogLevel.Error, ex.Message);
				return false;				
			}

			
		}

		public TimeSpan GetCurrentTime()
		{
			if (_waveStream == null)
			{
				return TimeSpan.Zero;
			}

			return _waveStream.CurrentTime;

		}

		public TimeSpan GetTotalDuration()
		{
			if (_waveStream == null)
			{
				return TimeSpan.Zero;
			}

			return _waveStream.TotalTime;
		}

		public void SetCurrentTime(TimeSpan timeSpan)
		{
			if(_waveStream == null)
			{
				return;
			}
			_waveStream.CurrentTime = timeSpan;
			foreach (var obj in _waveStreamList)
			{
				obj.CurrentTime = timeSpan;
			}
		}

		public PlayBackState GetPlayBackState()
		{
			if (_device != null)
				return (PlayBackState)_device.PlaybackState;
			else
				return PlayBackState.Stopped;

		}

		public void PlayAudio()
		{
			if (_device == null)
			{
				return;
			}
				_device.Play();

		}

		public void PauseAudio()
		{
			if (_device == null)
			{
				return;
			}
			_device.Pause();
		}

		public void StopAudio()
		{
			if (_device == null)
			{
				return;
			}
			_device.Stop();
		}

		private void _waveOutDevice_PlaybackStopped(object sender, EventArgs e)
		{
			PlaybackStop?.Invoke(e);
		}

		public void Dispose()
		{
			if (_device != null)
			{
				_device.Stop();
				_device.Dispose();
				_device = null;
			}

			if (_waveStream != null)
			{
				_waveStream.Close();
				_waveStream = null;
			}

			GC.Collect();
		}

		public void MixWaveProviders(IList<WavePlayer> inputs)
		{
			try
			{
				_mixer = new MixingWaveProvider32(inputs.Select(c => c._channels));

				_waveStream = inputs[0]._reader;
				foreach (var obj in inputs)
				{
					_waveStreamList.Add(obj._reader);
				}
				
				_device.Init(_mixer);

			}
			catch (Exception exception)
			{
				MessageBox.Show("All incoming channels must have the same format", "Errror", MessageBoxButton.OK, MessageBoxImage.Error);
				throw new ArgumentException(exception.Message, exception);
			}
		}

		public void FadeInOut(WavePlayer fadein, WavePlayer fadeout, int milliSeconds)
		{
			var fadeInProvider = fadein._reader.ToSampleProvider();
			var fadeOutProvider = fadeout._reader.ToSampleProvider();

			_mixer.RemoveInputStream(fadeout._channels);
			_mixer.RemoveInputStream(fadein._channels);

			var fadeOutObject = new FadeInOutSampleProvider(fadeOutProvider);
			var fadeInObject = new FadeInOutSampleProvider(fadeInProvider);
			
			var waveStream = new WaveProviderToWaveStream(fadeOutObject.ToWaveProvider());
			var waveStream2 = new WaveProviderToWaveStream(fadeInObject.ToWaveProvider());

			fadeout._channels = new WaveChannel32(waveStream);
			fadein._channels = new WaveChannel32(waveStream2);

			_mixer.AddInputStream(fadeout._channels);
			_mixer.AddInputStream(fadein._channels);
			
			fadeOutObject.BeginFadeOut(milliSeconds);
			fadeInObject.BeginFadeIn(milliSeconds);
		}

		public void FadeIn(WavePlayer fadein, double time)
		{
			var fadeInProvider = fadein._reader.ToSampleProvider();
			_mixer.RemoveInputStream(fadein._channels);
			var fadeInObject = new DelayFadeOutSampleProvider(fadeInProvider);			
			var waveStream2 = new WaveProviderToWaveStream(fadeInObject.ToWaveProvider());			
			fadein._channels = new WaveChannel32(waveStream2);			
			_mixer.AddInputStream(fadein._channels);
			fadeInObject.BeginFadeIn(time);
		}

		private DelayFadeOutSampleProvider RemoveAddStreamsFromMixer(WavePlayer fadeout)
		{
			var fadeOutProvider = fadeout._reader.ToSampleProvider();
			_mixer.RemoveInputStream(fadeout._channels);
			var fadeOutObject = new DelayFadeOutSampleProvider(fadeOutProvider);
			var waveStream = new WaveProviderToWaveStream(fadeOutObject.ToWaveProvider());
			fadeout._channels = new WaveChannel32(waveStream);
			_mixer.AddInputStream(fadeout._channels);
			return fadeOutObject;
		}
		public void FadeOut(WavePlayer fadeout, double time)
		{
			var fadeoutObject = RemoveAddStreamsFromMixer(fadeout);
			fadeoutObject.BeginFadeOut(2, time);
		}
		public void FadersInOut(WavePlayer fadeout, double time)
		{
			var fadeoutObject = RemoveAddStreamsFromMixer(fadeout);
			fadeoutObject.BeginFadersInOut(0, time);
		}

		public void CrossFadeAtEnds(WavePlayer crossfade)
		{
			var fadeoutObject = RemoveAddStreamsFromMixer(crossfade);
			fadeoutObject.BeginCrossFadeAtEnds();
		}

		public void	CopyofSampleProvider(WavePlayer wavePlayer, float selectbegintime, float selectendTime, float crossfadeDuration)
		{
			ISampleProvider copyofSampleProvider = null;
			 wavePlayer._reader.Seek(0, SeekOrigin.Begin);
			var sampleProvider = wavePlayer._reader.ToSampleProvider();
			
			var fadeSampleCount = (int)((crossfadeDuration * wavePlayer._reader.WaveFormat.SampleRate) / 1000);
			copyofSampleProvider = sampleProvider.Skip(TimeSpan.FromSeconds(selectbegintime));

			var readCount = fadeSampleCount > DelayFadeOutSampleProvider._copyofNotPlayingSammplesCapture?.Length ? fadeSampleCount :
				DelayFadeOutSampleProvider._copyofNotPlayingSammplesCapture.Length;

			//DelayFadeOutSampleProvider._copyofNotPlayingSammplesCapture = new float[2400];
			copyofSampleProvider.Read(DelayFadeOutSampleProvider._copyofNotPlayingSammplesCapture, 0, readCount);

			CopyofSampleProvider(wavePlayer, selectbegintime);

			DelayFadeOutSampleProvider._regionEndTimeInMillis = selectendTime * 1000;
			DelayFadeOutSampleProvider._regionStartTimeInMillis = selectbegintime * 1000;
		}

		public void CopyofSampleProvider(WavePlayer wavePlayer, float selectbegintime)
		{
			wavePlayer._reader.Seek(0, SeekOrigin.Begin);
			var xx = wavePlayer._reader.ToSampleProvider();
			DelayFadeOutSampleProvider._copyofnonplayingsourceprovider = xx.Skip(TimeSpan.FromSeconds(selectbegintime));
		}

		public void UpdateSelectionTimesonMouseClicks(float selectbegintime, float selectendtime)
		{
			DelayFadeOutSampleProvider._regionEndTimeInMillis = selectendtime * 1000;
			DelayFadeOutSampleProvider._regionStartTimeInMillis = selectbegintime * 1000;
		}

		public void FadeoutOnMouseClicksAndPause(WavePlayer fadeout, double time)
		{
			var fadeoutObject = RemoveAddStreamsFromMixer(fadeout);
			fadeoutObject.BeginFadeOutOnMouseClicksAndPause(2, time);
		}
	}
	

	public class WavePlayer :IDisposable
	{
		public WaveChannel32 _channels { get; set; }
		public WaveFileReader _reader;
		
		public WavePlayer()
		{

		}

		public WavePlayer(string FileName)
		{
			_reader = new WaveFileReader(FileName);
			_channels = new WaveChannel32(_reader) { PadWithZeroes = false };
		}
		
		public bool CompareSampleRates(string refWavFile, IList<string> sampleWavFiles)
		{
			var refReader = new WaveFileReader(refWavFile);
			
			var result = false;
			var count = 0;

			var diffSampleRatesIndex = new int[13];
				
			if (sampleWavFiles.Count() == 1)
			{
				var reader = new WaveFileReader(sampleWavFiles[0]);
				if (reader.WaveFormat.SampleRate == refReader.WaveFormat.SampleRate)
				{
					result = true;
				}
				else
				{
                    sampleWavFiles[0] = "Deleted";
                    result = false;
				}
				return result;
			}
			else
			{
                sampleWavFiles.ToList().ForEach(files =>
                {
                    var reader = new WaveFileReader(files);

                    if (reader.WaveFormat.SampleRate != refReader.WaveFormat.SampleRate)
                    {
                        sampleWavFiles[count] = "Deleted";
                        result = false;
                    }
                    else
                    {
                        result = true;
                    }

                    count++;

                });

            }

			return result;
		}

		public void SetVolumeStream(float volume)
		{
			_channels.Volume = volume;
		}

		public float GetVolume()
		{
			return _channels.Volume;
		}

			
		public void Dispose()
		{

			if (_channels != null)
			{
				_channels.Dispose();
				_reader.Dispose();
			}
		}
	}

		public class WaveProviderToWaveStream : WaveStream
		{
			private readonly IWaveProvider source;
			private long position;

			public WaveProviderToWaveStream(IWaveProvider source)
			{
				this.source = source;
			}

			public override WaveFormat WaveFormat
			{
				get { return source.WaveFormat; }
			}

			/// <summary>
			/// Don't know the real length of the source, just return a big number
			/// </summary>
			public override long Length
			{
				get { return Int32.MaxValue; }
			}

			public override long Position
			{
				get
				{
					// we'll just return the number of bytes read so far
					return position;
				}
				set
				{
					Position = value;
				}
			}

			public override int Read(byte[] buffer, int offset, int count)
			{			
				int read = source.Read(buffer, offset, count);
				position += read;
				return read;
			}
		}
	
	}

