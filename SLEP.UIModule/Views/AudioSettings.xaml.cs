using SLEP.Audio;
using SLEP.Logger;
using SLEP.Models;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace SLEP.UIModule.Views
{
	/// <summary>
	/// Interaction logic for AudioSettings.xaml
	/// </summary>
	public partial class AudioSettings : UserControl
	{
		private int _driverIndex;
		private int _latency;
		private int _crossFadeDuration;
		private int _numberOfBuffers;
		private string _asioDriver;
		private bool _exclusiveMode;
		private bool _xvfades;
		private AudioSettingsModel _settingsObject;
		
		public AudioSettings()
		{
			InitializeComponent();
			_settingsObject = AudioSettingsModel.GetInstance;
			GetAsioDrivers();
			LoadModelValues();
		}

		private void LoadModelValues()
		{
			_settingsObject.DeserializeAudioSettings();

			LenTb.Text = _settingsObject.CrossfadeTime.ToString();
			LatencyCB.SelectedValue = _settingsObject.Latency.ToString();

			if(_settingsObject.OutputDriver == 0)
			{
				WaveBtn.IsChecked = true;
				BuffersTB.Text = _settingsObject.NumberOfBuffers.ToString();
			}
			else if (_settingsObject.OutputDriver == 1)
			{
				WasBtn.IsChecked = true;
			}
			else if (_settingsObject.OutputDriver == 2)
			{
				AsioBtn.IsChecked = true;
			}

			if(_settingsObject.ExclusiveMode == true )
			{
					ExclCkB.IsChecked = true;
			}
			else
			{
				ExclCkB.IsChecked = false;
			}
			ExclCkB.IsEnabled = (WasBtn.IsChecked == true) ? true : false;

			BuffersTB.IsEnabled = (WaveBtn.IsChecked == true) ? true : false;

			if (WaveBtn.IsChecked == true)
			{
				label2.IsEnabled = true;
				BuffersTB.IsEnabled = true;
			}
			else
			{
				label2.IsEnabled = false;
				BuffersTB.IsEnabled = false;
			}


			if(AsioBtn.IsChecked == true)
			{
				AsioCB.IsEnabled = true;
				//AsioCB.SelectedIndex = 0;
				AsioCB.Text = _settingsObject.AsioDriver;
			}
			else
			{
				AsioCB.IsEnabled = false;
				AsioCB.SelectedIndex = -1;
			}

			if(_settingsObject.XVFades)
			{
				XFade.IsChecked = true;
			}
			else
			{
				VFade.IsChecked = true;
			}

			AudioSettngsGb.IsEnabled = _settingsObject.SessionCreatedFlag ? false : true;
		} 

		private static bool IsTextAllowed(string text)
		{
			Regex regex = new Regex("[^0-9]+"); //regex that matches disallowed text
			return !regex.IsMatch(text);
		}

		private bool ValidateDataEntered()
		{
			if (IsTextAllowed(LenTb.Text) == false)
			{
				MessageBox.Show("Invalid Characters entered, The Length Field will get reset to its previous value", "Info", MessageBoxButton.OK, MessageBoxImage.Asterisk);
				LenTb.Text = _settingsObject.CrossfadeTime.ToString();
				return false;
			}

			if (IsTextAllowed(BuffersTB.Text) == false)
			{
				MessageBox.Show("Invalid Characters entered", "Info", MessageBoxButton.OK, MessageBoxImage.Asterisk);
				return false;
			}

			return true;
		}
		private void SetAudioSettings()
		{
			if (WaveBtn.IsChecked == true)
			{
				_driverIndex = 0;
			}
			else if (WasBtn.IsChecked == true)
			{
				_driverIndex = 1;
			}			
			else if(AsioBtn.IsChecked == true)
			{
				_driverIndex = 2;
			}
						
			_latency = Convert.ToInt32(LatencyCB.SelectedValue);
			
			if(AsioBtn.IsChecked == true)
			{
				_asioDriver = AsioCB.SelectedValue.ToString();
			}

			if (ExclCkB.IsChecked == true)
			{
				_exclusiveMode = true;
			}
			else
			{
				_exclusiveMode = false;
			}

			if(XFade.IsChecked == true)
			{
				_xvfades = true;
			}
			else if(VFade.IsChecked == true)
			{
				_xvfades = false;
			}


			_numberOfBuffers =  BuffersTB.IsEnabled? Convert.ToInt32(BuffersTB.Text): 0;
			_crossFadeDuration = Convert.ToInt32(LenTb.Text);
		}
		private void OnOk_Clicked(object sender, RoutedEventArgs e)
		{
			if (!ValidateDataEntered())
			{
				return;
			}
			SetAudioSettings();
			
			if (_settingsObject == null)
			{
				Log.Write(LogLevel.Error, "The Settings Object is not initialized");
				CloseWindow(false);
			}
			_settingsObject.CrossfadeTime = _crossFadeDuration;
			_settingsObject.Latency = _latency;
			_settingsObject.OutputDriver = _driverIndex;
			_settingsObject.AsioDriver = _asioDriver;
			_settingsObject.ExclusiveMode = _exclusiveMode;
			_settingsObject.XVFades = _xvfades;
			_settingsObject.NumberOfBuffers = _numberOfBuffers;

			if(ExclCkB.IsEnabled == true && _exclusiveMode == true)
			{
				Log.Write(LogLevel.Info, "WASAPI Option is Selected with Exclusive Mode ON");
			}
			else if (ExclCkB.IsEnabled == true && _exclusiveMode == false)
			{
				Log.Write(LogLevel.Info, "WASAPI Option is Selected with Exclusive Mode OFF");
			}

			if(AsioCB.IsEnabled == true)
			{
				var msg = string.Format("ASIO Option is selected with {0} driver", _asioDriver);
				Log.Write(LogLevel.Info, msg);
			}
			
			_settingsObject.SerializeAudioSettings();

			CloseWindow(true);
		}

		private void OnWasapi_Click(object sender, RoutedEventArgs e)
		{
		
			ExclCkB.IsEnabled = true;
			AsioCB.IsEnabled = false;
			BuffersTB.IsEnabled = false;
		}

		private void OnWave_Clicked(object sender, RoutedEventArgs e)
		{
			Log.Write(LogLevel.Info, "WaveOut Option is Selected");
			ExclCkB.IsEnabled = false;
			AsioCB.IsEnabled = false;
			BuffersTB.IsEnabled = true;
		}

		private void GetAsioDrivers()
		{
			Log.Write(LogLevel.Info, "Getting the ASIO drivers.....");
			using (var asioObject = new AsioAudioService(""))
			{
				var asioDrivers = asioObject.GetDriverNames();
				AsioCB.Items.Clear();
				foreach (var obj in asioDrivers)
				{
					AsioCB.Items.Add(obj);
				}
			}
		}
		private void OnAsio_Clicked(object sender, RoutedEventArgs e)
		{
			AsioCB.IsEnabled = true;		
			AsioCB.SelectedIndex = 0;
			ExclCkB.IsEnabled = false;
			BuffersTB.IsEnabled = false;
		}
		private void OnCancel_Clicked(object sender, RoutedEventArgs e)
		{
			CloseWindow(false);
		}

		private void CloseWindow(bool flag)
		{
			var parentWindow = (Window)this.Parent;
			parentWindow.DialogResult = flag;
			parentWindow.Close();
		}
	
	}
}
