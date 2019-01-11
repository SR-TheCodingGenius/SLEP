using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SLEP.Models
{
	[Serializable]
	public class AudioSettingsModel
	{
		private static AudioSettingsModel _settingsObject;
		public static AudioSettingsModel GetInstance
		{
			get
			{
				
				if(_settingsObject == null)
				{
					_settingsObject = new AudioSettingsModel();
					_settingsObject.DeserializeAudioSettings();
				}

				return _settingsObject;
			}
			
		}

		public int OutputDriver = 0; //Waveout O/p driver
		public int Latency = 300; //Latency period in msec
		public int CrossfadeTime = 10; // Crossfade duration in msec
		public int NumberOfBuffers = 4; // Number of buffers for WaveOutEvent driver
		public string AsioDriver = ""; // ASIO driver name if installed
		public bool ExclusiveMode = false; // WASAPI Exclusive mode (true) or Shared mode (false)
		public bool XVFades = true;
		public bool SessionCreatedFlag = false;
		public void SerializeAudioSettings()
		{
			using (var file = File.Create("AudioSettings.bfp"))
			{
				var binaryFormatter = new BinaryFormatter();
				binaryFormatter.Serialize(file, this);
				file.Close();
			}
		}

		public void DeserializeAudioSettings()
		{
			if (!File.Exists("AudioSettings.bfp"))
				return;

			
			using (var file = File.Open("AudioSettings.bfp", FileMode.Open))
			{
				var bf = new BinaryFormatter();
				var tempObj = bf.Deserialize(file) as AudioSettingsModel;
				this.CrossfadeTime = tempObj.CrossfadeTime;
				this.Latency = tempObj.Latency;
				this.OutputDriver = tempObj.OutputDriver;
				this.AsioDriver = tempObj.AsioDriver;
				this.ExclusiveMode = tempObj.ExclusiveMode;
				this.NumberOfBuffers = tempObj.NumberOfBuffers;
				this.XVFades = tempObj.XVFades;
				file.Close();				
			}
		}


	}
	
}
