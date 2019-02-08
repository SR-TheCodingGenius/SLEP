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
		private static double fadeDurationInMillis = 0.0;
		private static float[] _fadeoutVolumes;
		private static float[] _fadeinVolumes;
		private static double timetoSkip = 0;
		private static float copyofStartTimeinMillis = 0.0f;
		private static double duration = 0.0;
		private bool bufferOverrideflag = false;
		private static int offsetbytes = 0;
		private static int copyofOffsetSamples = 0;
		public static float _regionEndTimeInMillis = 0.0f;
		public static float _regionStartTimeInMillis = 0.0f;
		public static int _triStateFlag = -1;
		public static int _addReadSamples = 0;
		public static ISampleProvider _copyofnonplayingsourceprovider;
		public static float[] _copyofNotPlayingSammplesCapture;

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
			lock (lockObject)
			{
				fadeSamplePosition = 0;
				fadeDurationInMillis = fadeDurationInMilliseconds;
				fadeSampleCount = (int)((fadeDurationInMilliseconds * source.WaveFormat.SampleRate * source.WaveFormat.Channels) / 1000);
				fadeState = FadeState.FadingIn;
				_fadeDuration = fadeDurationInMilliseconds;
							
			}
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
			
			if (_fadeoutVolumes == null && _fadeinVolumes == null)
			{
				_fadeoutVolumes = new float[fadeSampleCount];
				_fadeinVolumes = new float[fadeSampleCount];
			}

			CalculateFadeInOutVolumes();
		}

        public void BeginCrossFadeAtEnds()
        {
            fadeState = FadeState.CrossFade;
        }

		private void ExecuteWhenStateTransitionsToCrossFade(float[] buffer, int sampleCount)
		{
			if (!_nondivisibleFlag)
			{
				var time = bytesRead * 1000 / source.WaveFormat.SampleRate * source.WaveFormat.Channels;
				source = source.Skip(TimeSpan.FromMilliseconds(timetoSkip + time));
				bytesRead = 0;
			}
			else
			{
				var tempBuffer = new float[copyofOffsetSamples];
				source = source.Skip(TimeSpan.FromMilliseconds(timetoSkip));
				
				source.Read(tempBuffer, 0, copyofOffsetSamples);

				var mod = bytesRead % sampleCount;
				if (mod == 0)
				{
					var times = bytesRead / sampleCount;
					for (int count = 0; count < times; count++)
					{
						source.Read(buffer, 0, sampleCount);
					}
				}
				else
				{
					while(bytesRead > sampleCount)
					{
						source.Read(buffer, 0, sampleCount);
						bytesRead -= sampleCount;
					}
					source.Read(buffer, 0, bytesRead);
				}
				
				_nondivisibleFlag = false;
				
			}
			bufferOverrideflag = false;
			timetoSkip = 0;
			fadeState = FadeState.FullVolume;
		}

		static int countMethod = 0;
		public int Read(float[] buffer, int offset, int count)
		{
			int sourceSamplesRead = 0;

			if (fadeState == FadeState.CrossFade)
			{
				ExecuteWhenStateTransitionsToCrossFade(buffer, count);
			}

			sourceSamplesRead = source.Read(buffer, offset, count);

			// initializes buffer for crossfade operation.
			if (_copyofNotPlayingSammplesCapture == null)
			{
				var bytes = fadeSampleCount > sourceSamplesRead ? fadeSampleCount : sourceSamplesRead;
				_copyofNotPlayingSammplesCapture = new float[bytes];
			}

			// gets called after the audio pointer is moved to starting of the region
			if (copyofOffsetSamples > 0 && !bufferOverrideflag)
			{
				var offsetSamples = bytesRead;
				//OverrideOffsetSamples(buffer, offsetSamples, sourceSamplesRead);
				copyofOffsetSamples = 0;
				bytesRead = 0;
			}

			// calculates number of samples with in the region and converts into time duration
			_addReadSamples += sourceSamplesRead;
			duration = (_addReadSamples / (double)source.WaveFormat.SampleRate * source.WaveFormat.Channels) * 1000.0;

			if (fadeOutDelaySamples > 0)
			{
				fadeOutDelayPosition += sourceSamplesRead / WaveFormat.Channels;
				if (fadeOutDelayPosition >= fadeOutDelaySamples)
				{
					fadeOutDelaySamples = 0;
				}
			}

			// over-rides samples with beginning of the region, this gets called  after crossfade.
			if (bufferOverrideflag)
			{
				ReadExtraBlocks(buffer, count);
				countMethod++;
			}

			if (fadeState == FadeState.FadingIn)
			{
				FadeIn(buffer, offset, sourceSamplesRead);
			}
			else if (fadeState == FadeState.FadingOut)
			{
				// initiates fadeout at end of the region in non-loop.
				if (_regionStartTimeInMillis + duration >= _regionEndTimeInMillis)
				{
					FadeOut(buffer, offset, sourceSamplesRead);
					_addReadSamples = 0;
					_triStateFlag = 1;
				}
			}
			else if (fadeState == FadeState.FadingoutOnMouseClicksAndPause)
			{
				// initiates fadeout at mouse clicks and pause events
				FadeOut(buffer, offset, sourceSamplesRead);
			}
			else if (fadeState == FadeState.Silence)
			{
				ClearBuffer(buffer, offset, count);
			}
			else if (fadeState == FadeState.FadingInOut)
			{
				//initiates crossfade at the loop endings
				FadeOutAtLast(buffer, offset, count);
				_triStateFlag = 1;
				copyofStartTimeinMillis = _regionStartTimeInMillis;
			}

			return sourceSamplesRead;
		}

		static int bytesRead = 0;
		/// <summary>
		/// Reads extra data blocks (if any) after applying crossfade algorithm
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="count"></param>
		private void ReadExtraBlocks(float [] buffer, int count)
		{

			int length = (offsetbytes > count) ? offsetbytes : count;
			
			var temporaryArray = new float[length];

			if (offsetbytes > 0)
			{
				var offsetSamplesRead = _copyofnonplayingsourceprovider.Read(temporaryArray, 0, offsetbytes);
			}

			var sourceSamplesRead = _copyofnonplayingsourceprovider.Read(temporaryArray, 0, count);
			for (int i = 0; i < sourceSamplesRead; i++)
			{
				buffer[i] = temporaryArray[i];
			}

			bytesRead += sourceSamplesRead;
			offsetbytes = 0;
		}

		private static void ClearBuffer(float[] buffer, int offset, int count)
		{
			int n = 0;
			for ( n = 0; n < count; n++)
			{
				buffer[n + offset] = 0;
			}
		}
		
		private void FadeOutAtLast(float[] buffer, int offset, int sourceSamplesRead)
		{
			int sample = 0;
			while(sample < sourceSamplesRead)
			{
				// Crossfade Logic....
				buffer[sample] *= _fadeoutVolumes[fadeSamplePosition];
				buffer[sample] += (_copyofNotPlayingSammplesCapture[fadeSamplePosition] * _fadeinVolumes[fadeSamplePosition]);

				fadeSamplePosition++;
				sample++;

				if (fadeSamplePosition >= fadeSampleCount)
				{
					// Fill remaining samples after Crossfade Logic....
					timetoSkip = FillBufferWithSamples(buffer, sample + offset, sourceSamplesRead - sample);
					fadeState = FadeState.FullVolume;
					break;
				}
			}
		}

		static bool _nondivisibleFlag = false;
		private double  FillBufferWithSamples(float[] buffer, int offset, int count)
		{	
			int n = 0 ;
			int time = 0;
			for (n = 0; n < count; n++)
			{
				buffer[n + offset] = _copyofNotPlayingSammplesCapture[n + offset];
			}

			bufferOverrideflag = true;
			fadeState = FadeState.FullVolume;

			var mod = (count * 1000) % source.WaveFormat.SampleRate * source.WaveFormat.Channels;

			_nondivisibleFlag = mod != 0 ? true : false;
			if (_nondivisibleFlag)
			{
				offsetbytes = count;
				copyofOffsetSamples = count;
			}
			else
			{
				time = (count * 1000) / source.WaveFormat.SampleRate * source.WaveFormat.Channels;
				offsetbytes = 0;
				copyofOffsetSamples = 0;
			}
			_copyofnonplayingsourceprovider = _copyofnonplayingsourceprovider.Skip(TimeSpan.FromMilliseconds(fadeDurationInMillis + time));
			return fadeDurationInMillis + time;
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
		private void CalculateFadeInOutVolumes()
		{
			int sample = 0;
			float multiplier = 0.0f;

			// Calculate Fade-out Volumes
			while (sample < fadeSampleCount)
			{

				if (_settingsObject.XVFades == true)
				{
					var fadeoutX = (sample / (float)fadeSampleCount);
					multiplier = _crossFadeInstance.FadeOutVolume(fadeoutX);
				}
				else
				{
					multiplier = 1.0f - (sample / (float)fadeSampleCount);
				}

				_fadeoutVolumes[sample] = multiplier;
				sample++;				
			}			
			sample = 0;
			multiplier = 0.0f;
			// Calculate Fade-in Volumes
			while (sample < fadeSampleCount)
			{
				if (_settingsObject.XVFades == true)
				{
					var fadeinX = (sample / (float)fadeSampleCount);
					multiplier = _crossFadeInstance.FadeInVolume(fadeinX);

				}
				else
				{
					multiplier = (sample / (float)fadeSampleCount);
				}

				_fadeinVolumes[sample] = multiplier;
				sample++;
			}
			

		}

		
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
