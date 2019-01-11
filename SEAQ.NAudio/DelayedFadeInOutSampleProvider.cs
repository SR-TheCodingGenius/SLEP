using NAudio.Wave;
using SLEP.CrossFadeAlgo;
using SLEP.Models;
using System;
using System.IO;

namespace SLEP.Audio
{
	public class DelayFadeOutSampleProvider : ISampleProvider
	{
		enum FadeState
		{
			Silence,
			FadingIn,
			FullVolume,
			FadingOut,
			FadingInOut,
			CrossFade,
			FadingoutOnMouseClicksAndPause
		}

		private readonly object lockObject = new object();
		private ISampleProvider source;
		private int fadeSamplePosition;
		private int fadeSampleCount;
		private int fadeOutDelaySamples;
		private int fadeOutDelayPosition;
		private FadeState fadeState;
		private CrossFade _crossFadeInstance;
		private AudioSettingsModel _settingsObject = AudioSettingsModel.GetInstance;
		private double _fadeDuration = 0.0;
		public static float _regionEndTimeInMillis = 0.0f;
		public static float _regionStartTimeInMillis = 0.0f;
		public static int _triStateFlag = -1;
		private static int _samplesinTimeDuration = 0;
		public DelayFadeOutSampleProvider(ISampleProvider source, bool initiallySilent = false)
		{
			this.source = source;
			this.fadeState = initiallySilent ? FadeState.Silence : FadeState.FullVolume;
			if (_settingsObject.XVFades)
			{
				_crossFadeInstance = new CrossFade();
			}
		}

		public void BeginFadeIn(double fadeDurationInMilliseconds)
		{
			//lock (lockObject)
			//{
				fadeSamplePosition = 0;
				fadeSampleCount = (int)((fadeDurationInMilliseconds * source.WaveFormat.SampleRate * source.WaveFormat.Channels) / 1000);
				fadeState = FadeState.FadingIn;
				_fadeDuration = fadeDurationInMilliseconds;
				if (_copyofNotPlayingSammplesCapture == null)
				{
					_copyofNotPlayingSammplesCapture = new float[fadeSampleCount];
				}
				if (_fadeinVolumes == null && _fadeoutVolumes == null)
				{
					_fadeoutVolumes = new float[fadeSampleCount];
					_fadeinVolumes = new float[fadeSampleCount];
				}				
			//}
		}

		public void BeginFadeOut(double fadeAfterMilliseconds, double fadeDurationInMilliseconds)
		{
			fadeSamplePosition = 0;
			fadeSampleCount = (int)((fadeDurationInMilliseconds * source.WaveFormat.SampleRate * source.WaveFormat.Channels) / 1000);
			fadeOutDelaySamples = (int)((fadeAfterMilliseconds * source.WaveFormat.SampleRate * source.WaveFormat.Channels) / 1000);
			fadeOutDelayPosition = 0;
			fadeState = FadeState.FadingOut;
		}

		public void BeginFadeOutOnMouseClicksAndPause(double fadeAfterMilliseconds, double fadeDurationInMilliseconds)
		{
			fadeSamplePosition = 0;
			fadeSampleCount = (int)((fadeDurationInMilliseconds * source.WaveFormat.SampleRate * source.WaveFormat.Channels) / 1000);
			fadeOutDelaySamples = (int)((fadeAfterMilliseconds * source.WaveFormat.SampleRate * source.WaveFormat.Channels) / 1000);
			fadeOutDelayPosition = 0;
			fadeState = FadeState.FadingoutOnMouseClicksAndPause;
		}

		public void BeginFadersInOut(double fadeAfterMilliseconds, double fadeDurationInMilliseconds)
		{
			fadeSamplePosition = 0;
			fadeSampleCount = (int)((fadeDurationInMilliseconds * source.WaveFormat.SampleRate) / 1000);
			fadeOutDelaySamples = (int)((fadeAfterMilliseconds * source.WaveFormat.SampleRate) / 1000);
			fadeOutDelayPosition = 0;
			fadeState = FadeState.FadingInOut;
		}

        public void BeginCrossFadeAtEnds()
        {
            fadeState = FadeState.CrossFade;
        }

        FadeState previousFadeState;
		static int fadestateCount = 0;
		public static int _addReadSamples = 0;
		static float copyofStartTimeinMillis = 0.0f;
		static double duration = 0.0;
		double diff = 0.0;
		public int Read(float[] buffer, int offset, int count)
		{
			int sourceSamplesRead = 0;
			_samplesinTimeDuration = (int)((_regionEndTimeInMillis - _regionStartTimeInMillis) * source.WaveFormat.SampleRate * source.WaveFormat.Channels) / 1000;
			if (fadeState == FadeState.CrossFade)
			{
				source = source.Skip(TimeSpan.FromMilliseconds(10));
				fadeState = FadeState.FullVolume;
				//_addReadSamples = 0;
			}
			
			sourceSamplesRead = source.Read(buffer, offset, count);
		//	if(fadeState == FadeState.FullVolume)
			{
				_addReadSamples += sourceSamplesRead;
			}
			duration = (_addReadSamples / (double)source.WaveFormat.SampleRate) * 1000.0;
			
			if (fadeOutDelaySamples > 0)
			{
				fadeOutDelayPosition += sourceSamplesRead / WaveFormat.Channels;
				if (fadeOutDelayPosition >= fadeOutDelaySamples)
				{
					fadeOutDelaySamples = 0;					
				}
			}
			
			if (fadeState == FadeState.FadingIn)
			{
				if (copyofStartTimeinMillis != 0.0f && Math.Abs(copyofStartTimeinMillis - _regionStartTimeInMillis) >= 100.0f)
				{
					_addReadSamples = 0;
				}
				FadeIn(buffer, offset, sourceSamplesRead);
			}
			else if (fadeState == FadeState.FadingOut)
			{
				if (_regionStartTimeInMillis + duration >= _regionEndTimeInMillis)
				{				
					FadeOut(buffer, offset, sourceSamplesRead);
					_addReadSamples = 0;
					_triStateFlag = 1;
				}				
			}
			else if (fadeState == FadeState.FadingoutOnMouseClicksAndPause)
			{
				FadeOut(buffer, offset, sourceSamplesRead);
			}
			else if (fadeState == FadeState.FadingInOut)
			{
				FadersInLoop(buffer, offset, sourceSamplesRead);
			}
			else if (fadeState == FadeState.Silence)
			{
				ClearBuffer(buffer, offset, count);
			}
			else if (fadeState == FadeState.FullVolume)
			{
				if (previousFadeState == FadeState.FadingInOut)
				{
					fadestateCount++;
					if (_regionStartTimeInMillis + duration >= _regionEndTimeInMillis)
					{
						diff = (_regionStartTimeInMillis + duration) - _regionEndTimeInMillis;
						_triStateFlag = 1;
						copyofStartTimeinMillis = _regionStartTimeInMillis;
						FadeOutAtLast(buffer, offset, count);
						previousFadeState = FadeState.FullVolume;
						_addReadSamples = 0;
					}
					//if (fadestateCount == 3)
					//{
					//	FadeOutAtLast(buffer, offset, count);
					//	fadestateCount = 0;
					//}
				}
			}
			return sourceSamplesRead;
		}

		private static void ClearBuffer(float[] buffer, int offset, int count)
		{
			int n = 0;
			for ( n = 0; n < count; n++)
			{
				buffer[n + offset] = 0;
			}
		}

		static float[] _fadeoutVolumes;
		static float[] _fadeinVolumes;
		private void FadeOutAtLast(float[] buffer, int offset, int sourceSamplesRead)
		{
			
			fadeSamplePosition = 0;
			var indexOffset = Math.Abs(sourceSamplesRead - fadeSampleCount);
			int sample = 0;
			while(sample < sourceSamplesRead)
			{
				buffer[indexOffset + sample] *= _fadeoutVolumes[fadeSamplePosition];
				buffer[indexOffset + sample] += (_copyofNotPlayingSammplesCapture[fadeSamplePosition] * _fadeinVolumes[fadeSamplePosition]);
				//buffer[sample] *= _fadeoutVolumes[fadeSamplePosition];
				//buffer[sample] += (_copyofNotPlayingSammplesCapture[fadeSamplePosition] * _fadeinVolumes[fadeSamplePosition]);
				fadeSamplePosition++;
				sample++;
				if(fadeSamplePosition >= fadeSampleCount)
				{					
					//ClearBuffer(buffer, sample + offset, sourceSamplesRead - sample);
					//fadeState = FadeState.Silence;
					break;
				}
			}
		}
		private void FadeOut(float[] buffer, int offset, int sourceSamplesRead)
		{
			int sample = 0;
			float multiplier = 0.0f;

			while (sample < sourceSamplesRead)
			{

				if (_settingsObject.XVFades == true)
				{
					var fadeoutX = (fadeSamplePosition / (float)fadeSampleCount);
					multiplier = _crossFadeInstance.FadeOutVolume(fadeoutX);
				}
				else
				{
					multiplier = 1.0f - (fadeSamplePosition / (float)fadeSampleCount);
				}

				for (int ch = 0; ch < source.WaveFormat.Channels; ch++)
				{
					buffer[offset + sample] *= multiplier;					
					sample++;
				}

			
				fadeSamplePosition++;
				if (fadeSamplePosition >= fadeSampleCount)
				{
					ClearBuffer(buffer, sample + offset, sourceSamplesRead - sample);
					fadeState = FadeState.Silence;
					break;
				}

			}
		}
		private void FadersInLoop(float[] buffer, int offset, int sourceSamplesRead)
		{
			int sample = 0;
			float multiplier = 0.0f;

			previousFadeState = FadeState.FadingInOut;
			while (sample <= sourceSamplesRead)
			{

				if (_settingsObject.XVFades == true)
				{
					var fadeoutX = (fadeSamplePosition / (float)fadeSampleCount);
					multiplier = _crossFadeInstance.FadeOutVolume(fadeoutX);
				}
				else
				{
					multiplier = 1.0f - (fadeSamplePosition / (float)fadeSampleCount);
				}
				if (fadeSamplePosition < fadeSampleCount)
				{
					_fadeoutVolumes[fadeSamplePosition] = multiplier;
				}
				
				fadeSamplePosition++;
				sample++;
				if (fadeSamplePosition >= fadeSampleCount)
				{
					fadeState = FadeState.FullVolume;
					break;
				}
			}
		}

		public static float[] _copyofNotPlayingSammplesCapture;
		private void FadeIn(float[] buffer, int offset, int sourceSamplesRead)
		{
			int sample = 0;
			float multiplier = 0.0f;
			
			while (sample < sourceSamplesRead)
			{
				if (_crossFadeInstance != null)
				{
					var fadeinX = (fadeSamplePosition / (float)fadeSampleCount);
					multiplier = _crossFadeInstance.FadeInVolume(fadeinX);
					
				}
				else
				{
					multiplier = (fadeSamplePosition / (float)fadeSampleCount);
				}
				for (int ch = 0; ch < source.WaveFormat.Channels; ch++)
				{
					buffer[offset + sample] *= multiplier;
					_fadeinVolumes[sample] = multiplier;
					sample++;
				}
				fadeSamplePosition++;
				if (fadeSamplePosition >= fadeSampleCount)
				{
					fadeState = FadeState.FullVolume;
					// no need to multiply any more
					break;
				}
			}
		}

		/// <summary>
		/// WaveFormat of this SampleProvider
		/// </summary>
		public WaveFormat WaveFormat
		{
			get { return source.WaveFormat; }
		}
	}
}
