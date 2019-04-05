namespace SLEP.Audio
{
	public class AudioFactory
	{
		public static IAudioService GetAudioObject(int objectType, string asioDriver, bool exclusiveMode)
		{
			IAudioService audiObject = null;

			if(objectType == 0)
			{
				audiObject = new AudioService();
			}
			else if(objectType == 1)
			{
				audiObject = new WasapiAudioService(exclusiveMode);
			}
			else if(objectType == 2)
			{
				audiObject = new AsioAudioService(asioDriver);
			}

			return audiObject;
		}
	}
}
