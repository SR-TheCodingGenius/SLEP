using NAudio.Wave;
using SLEP.CrossFadeAlgo;
using SLEP.Models;
using System;
using System.Collections.Generic;

namespace SLEP.Audio
{
	public class FadeInOutSampleProvider : ISampleProvider
	{
		enum FadeState
		{
			Silence,
			FadingIn,
			FullVolume,
			FadingOut,
		}

		private readonly object lockObject = new object();
		private readonly ISampleProvider source;
		private int fadeSamplePosition;
		private int fadeSampleCount;
		private FadeState fadeState;
		private CrossFade _crossFadeInstance;
		private AudioSettingsModel _settingsObject = AudioSettingsModel.GetInstance;
				
		/// <summary>
		/// Creates a new FadeInOutSampleProvider
		/// </summary>
		/// <param name="source">The source stream with the audio to be faded in or out</param>
		/// <param name="initiallySilent">If true, we start faded out</param>
		public FadeInOutSampleProvider(ISampleProvider source, bool initiallySilent = false)
		{
			this.source = source;
			this.fadeState = initiallySilent ? FadeState.Silence : FadeState.FullVolume;

			if (_settingsObject.XVFades)
			{
				_crossFadeInstance = new CrossFade();
			}

		}

		
		/// <summary>
		/// Requests that a fade-in begins (will start on the next call to Read)
		/// </summary>
		/// <param name="fadeDurationInMilliseconds">Duration of fade in milliseconds</param>
		public void BeginFadeIn(double fadeDurationInMilliseconds)
		{
			lock (lockObject)
			{
				fadeSamplePosition = 0;
				fadeSampleCount = (int)((fadeDurationInMilliseconds * source.WaveFormat.SampleRate) / 1000);
				fadeState = FadeState.FadingIn;
			}
		}

		/// <summary>
		/// Requests that a fade-out begins (will start on the next call to Read)
		/// </summary>
		/// <param name="fadeDurationInMilliseconds">Duration of fade in milliseconds</param>
		public void BeginFadeOut(double fadeDurationInMilliseconds)
		{
			lock (lockObject)
			{
				fadeSamplePosition = 0;
				fadeSampleCount = (int)((fadeDurationInMilliseconds * source.WaveFormat.SampleRate) / 1000);
				fadeState = FadeState.FadingOut;
			}
		}


		/// <summary>
		/// Reads samples from this sample provider
		/// </summary>
		/// <param name="buffer">Buffer to read into</param>
		/// <param name="offset">Offset within buffer to write to</param>
		/// <param name="count">Number of samples desired</param>
		/// <returns>Number of samples read</returns>
		public int Read(float[] buffer, int offset, int count)
		{
			int sourceSamplesRead = 0;
		
			sourceSamplesRead = source.Read(buffer, offset, count);
			if (fadeState != FadeState.FadingOut)
			{
				DelayFadeOutSampleProvider._addReadSamples += sourceSamplesRead;
			}

			if (fadeState == FadeState.FadingIn)
			{
				FadeIn(buffer, offset, sourceSamplesRead);
			}
			else if (fadeState == FadeState.FadingOut)
			{
				FadeOut(buffer, offset, sourceSamplesRead);
			}
			else if (fadeState == FadeState.Silence)
			{
				ClearBuffer(buffer, offset, count);
			}
			return sourceSamplesRead;
		}

		private static void ClearBuffer(float[] buffer, int offset, int count)
		{
			for (int n = 0; n < count; n++)
			{
				buffer[n + offset] = 0;
			}
		}

		private void FadeOut(float[] buffer, int offset, int sourceSamplesRead)
		{
			int sample = 0;
			float multiplier = 0.0f;

			while (sample < sourceSamplesRead)
			{
			
				if (_crossFadeInstance != null)
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
				if (fadeSamplePosition > fadeSampleCount - 1)
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
