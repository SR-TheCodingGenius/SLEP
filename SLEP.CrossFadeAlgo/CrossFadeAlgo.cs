using System;

namespace SLEP.CrossFadeAlgo
{
	public class CrossFadeAlgo
	{
		private static CrossFadeAlgo _instance;
		private float _velocity;
		private float _crossFadeTimeInMilliSecs;
		private float _stepSize;
		private float _x;
		private float _p;
		public static CrossFadeAlgo Instance
		{
			get
			{
				if(_instance == null)
				{
					_instance = new CrossFadeAlgo();					
				}
				return _instance;
			}			
		}
		private CrossFadeAlgo()
		{
		}

		public float CrossFadeTime
		{
			get { return _crossFadeTimeInMilliSecs; }
			set { _crossFadeTimeInMilliSecs = value; }
		}

		public float Velocity
		{
			get { return _velocity; }
			set { _velocity = value; }
		}

		public float X
		{
			get { return _x; }
			set { _x = value; }
		}

		public float P
		{
			get { return _p; }
			set { _p = value; }
		}

		public float Step
		{
			get { return _stepSize; }
			set { _stepSize = value; }
		}

		public float[] CrossFadeAlgorithm()
		{
			float[] volumes = new float[2];


			var fadeInEPowerValue = Math.Pow(Math.E, ((-1.0f * _velocity) * (_x - _p)));
			var fadeInVolume = 1.0f / (1.0f + fadeInEPowerValue);

			var fadeOutEPowerValue = Math.Pow(Math.E, ((-1.0f * _velocity) * ((1.0f - _x) - _p)));
			var fadeOutVolume = 1.0f / (1.0f + fadeOutEPowerValue);

			volumes[0] = (float)fadeInVolume;
			volumes[1] = (float)fadeOutVolume;
			_x = _x + _stepSize;

			return volumes;
		}

	}
}
