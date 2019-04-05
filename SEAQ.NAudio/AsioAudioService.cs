using System;
using System.Linq;
using System.Windows;
using NAudio.Wave;
using System.Collections.Generic;
using SLEP.NAudio.Enums;
using SLEP.Logger;
using System.IO;

namespace SLEP.Audio
{
	public class AsioAudioService : IAudioService, IDisposable
	{
			public delegate void PlayStopHandle(EventArgs e);
		private AsioOut _device;
		public string _audioFileName { get; set; }
		private WaveStream _waveStream;
		private IList<WaveStream> _waveStreamList;
		private string _asioDriverName;
		private static IList<IWaveProvider> _inputs;
		private MixingWaveProvider32 _mixer;
		public AsioAudioService(string driverName)
		{
			_asioDriverName = driverName;
			_inputs = new List<IWaveProvider>();
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

	
		public TimeSpan GetCurrentTime()
		{
			if (_waveStream == null)
			{
				return TimeSpan.Zero;
			}

			return _waveStream.CurrentTime;
		}

		public PlayBackState GetPlayBackState()
		{
			if (_device != null)
				return (PlayBackState)_device.PlaybackState;
			else
				return PlayBackState.Stopped;
		}

		public TimeSpan GetTotalDuration()
		{
			if (_waveStream == null)
			{
				return TimeSpan.Zero;
			}

		
			return _waveStream.TotalTime;
		}

		public string[] GetDriverNames()
		{
			var names = AsioOut.GetDriverNames();
			return names;			
		}
		public bool IntializeNAudioLibrary(int latency)
		{
			try
			{
				_device = new AsioOut(_asioDriverName);
				_device.ChannelOffset = 0;
				_waveStreamList = new List<WaveStream>();
				return true;
			}
			catch (Exception ex)
			{
				Log.Write(LogLevel.Error, ex.Message);
				return false;
			}
		}
		
		public void PauseAudio()
		{
			if (_device == null)
			{
				return;
			}
			_device.Pause();
		}

		public void PlayAudio()
		{
			if (_device == null)
			{
				return;
			}
			_device.Play();
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
		
		public void StopAudio()
		{
			if(_device == null)
			{
				return;
			}
			_device.Stop();
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
				Log.Write(LogLevel.Error, "File Name : AsioAudioService.cs, Method Name : MixWaveProviders(), Line Number : 131");
				throw new ArgumentException(exception.Message, exception);
			}

		}

		PlayBackState IAudioService.GetPlayBackState()
		{
			if (_device != null)
				return (PlayBackState)_device.PlaybackState;
			else
				return PlayBackState.Stopped;
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
			fadeoutObject.BeginFadersInOut(2, time);
		}

		public void CrossFadeAtEnds(WavePlayer crossfade)
		{
			var fadeoutObject = RemoveAddStreamsFromMixer(crossfade);
			fadeoutObject.BeginCrossFadeAtEnds();
		}

		public void CopyofSampleProvider(WavePlayer wavePlayer, float selectbegintime, float selectendTime, float crossfadeDuration)
		{
			ISampleProvider copyofSampleProvider = null;
			wavePlayer._reader.Seek(0, SeekOrigin.Begin);
			var sampleProvider = wavePlayer._reader.ToSampleProvider();

			var fadeSampleCount = (int)((crossfadeDuration * wavePlayer._reader.WaveFormat.SampleRate) / 1000);
			copyofSampleProvider = sampleProvider.Skip(TimeSpan.FromSeconds(selectbegintime));

			var readCount = fadeSampleCount > DelayFadeOutSampleProvider._copyofNotPlayingSammplesCapture.Length ? fadeSampleCount :
				DelayFadeOutSampleProvider._copyofNotPlayingSammplesCapture.Length;
			
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

	
}
