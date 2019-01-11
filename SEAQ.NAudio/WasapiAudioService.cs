﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using SLEP.NAudio.Enums;
using System.IO;
using SLEP.Logger;

namespace SLEP.Audio
{
	class WasapiAudioService : IAudioService
	{
		public event PlayStopHandle PlaybackStop;    
		public delegate void PlayStopHandle(EventArgs e);
		private WasapiOut _device;
		public string _audioFileName { get; set; }
		private WaveStream _waveStream;
		private IList<WaveStream> _waveStreamList;
		private bool _exclusiveMode;
		private MixingWaveProvider32 _mixer;
		public WasapiAudioService(bool mode)
		{
			_exclusiveMode = mode;
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

		public bool IntializeNAudioLibrary(int latency)
		{
			try
			{
				if (_exclusiveMode)
				{
					_device = new WasapiOut(AudioClientShareMode.Exclusive, false, latency);
				}
				else
				{
					_device = new WasapiOut(AudioClientShareMode.Shared, true, latency);
				}

				_waveStreamList = new List<WaveStream>();
				return true;
			}
			catch(Exception ex)
			{
				Log.Write(LogLevel.Error, ex.Message);
				return false;
			}
		}
		private void _wasapiDevice_PlaybackStopped(object sender, EventArgs e)
		{
			PlaybackStop?.Invoke(e);
		}
		

		public void PauseAudio()
		{
			if (_device == null)
			{
				return;
			}
			_device.Stop();
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
			_waveStream.CurrentTime = timeSpan;
			foreach (var obj in _waveStreamList)
			{
				obj.CurrentTime = timeSpan;
			}
		}

		public void StopAudio()
		{
			if (_device == null)
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
				MessageBox.Show("The Driver is already in use", "Errror", MessageBoxButton.OK, MessageBoxImage.Error);
				throw new ArgumentException(exception.Message, exception);
			}
		}

		public void FadeInOut(WavePlayer fadein, WavePlayer fadeout, int milliSeconds)
		{
			//DelayFadeOutSampleProvider._addReadSamples = 0;
			var fadeInProvider = fadein._reader.ToSampleProvider();
			var fadeOutProvider = fadeout._reader.ToSampleProvider();

			_mixer.RemoveInputStream(fadeout._channels);
			_mixer.RemoveInputStream(fadein._channels);


			var fadeOutObject = new FadeInOutSampleProvider(fadeOutProvider);
			var fadeInObject = new FadeInOutSampleProvider(fadeInProvider);
			//var fadeOutObject = new DelayFadeOutSampleProvider(fadeOutProvider);
			//var fadeInObject = new DelayFadeOutSampleProvider(fadeInProvider);

			var waveStream = new WaveProviderToWaveStream(fadeOutObject.ToWaveProvider());
			var waveStream2 = new WaveProviderToWaveStream(fadeInObject.ToWaveProvider());

			fadeout._channels = new WaveChannel32(waveStream);
			fadein._channels = new WaveChannel32(waveStream2);

			_mixer.AddInputStream(fadeout._channels);
			_mixer.AddInputStream(fadein._channels);

			//fadeOutObject.BeginFadeOut(0, milliSeconds);
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
			fadeoutObject.BeginFadeOut(0, time);
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

		public void CopyofSampleProvider(WavePlayer wavePlayer, float selectbegintime, float selectendTime, float crossfadeDuration)
		{
			ISampleProvider copyofSampleProvider = null;
			wavePlayer._reader.Seek(0, SeekOrigin.Begin);
			var sampleProvider = wavePlayer._reader.ToSampleProvider();
			var fadeSampleCount = (int)((crossfadeDuration * wavePlayer._reader.WaveFormat.SampleRate) / 1000);
			copyofSampleProvider = sampleProvider.Skip(TimeSpan.FromSeconds(selectbegintime));
			copyofSampleProvider.Read(DelayFadeOutSampleProvider._copyofNotPlayingSammplesCapture, 0, fadeSampleCount);

			DelayFadeOutSampleProvider._regionEndTimeInMillis = selectendTime * 1000;
			DelayFadeOutSampleProvider._regionStartTimeInMillis = selectbegintime * 1000;
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