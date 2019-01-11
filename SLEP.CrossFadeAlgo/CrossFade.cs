using SLEP.Logger;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SLEP.CrossFadeAlgo
{
	public class CrossFade
	{
		private const float Velocity = -20.0f;
		private const float P = 0.5f;
		private const float E = 2.718f;
				
		public CrossFade()
		{
						
		}
		public float FadeInVolume(float fadeinX)
		{
			var fadeInVolume = 0.0;
			var fadeInEPowerValue = Math.Pow(E, (Velocity * (fadeinX - P)));
			fadeInVolume = 1.0f / (1.0f + fadeInEPowerValue);

			return (float)fadeInVolume;
		}

		public float FadeOutVolume(float fadeoutX)
		{
			var fadeOutVolume = 0.0;
			var fadeOutEPowerValue = Math.Pow(E, (Velocity * ((1.0f - fadeoutX) - P)));
			fadeOutVolume = 1.0f / (1.0f + fadeOutEPowerValue);
		
			return (float)fadeOutVolume;
		}

	}
}
