using SLEP.Extensions.Handlers;
using SLEP.UIModule.Views;
using System.Windows;

namespace SLEP.UIModule.Menu
{

	public partial class OptionsMenuViewObject 
	{
		public OptionsMenuViewObject()
		{
			
			ABCTestView.TestModeOnOff += ABCTest_TestModeOnOff;
		}
		
		public void OnSettingsClicked()
		{
			Window window = new Window
			{
				Title = "Audio Settings Window",
				Content = new AudioSettings()
			};

			window.SetWindowProperties();
			window.ShowDialog();
		}
		private void ABCTest_TestModeOnOff(object sender, bool e)
		{
			//if (e == true)
			//{
			//	OptionsMenu.IsEnabled = false;
			//}
			//else
			//{
			//	OptionsMenu.IsEnabled = true;
			//}
		}

		public void OnCustomScalesClicked()
		{
			Window window = new Window
			{
				Title = "Custom Scales",
				Content = new CustomScales()
			};

			window.SetWindowProperties();
			window.ShowDialog();
		}
	}
}
