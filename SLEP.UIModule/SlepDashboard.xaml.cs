using SLEP.Extensions.Handlers;
using SLEP.Models;
using SLEP.UIModule.Views;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace SLEP.UIModule
{
	/// <summary>
	/// Interaction logic for SlepDashboard.xaml
	/// </summary>
	public partial class SlepDashboard : UserControl
	{
		private bool _controlsFlag = false;
		private IList<RecentSessions> _recentSessions;
			
		public SlepDashboard()
		{
			InitializeComponent();
			_recentSessions = LoadRecentFilesData();
		}

		private IList<RecentSessions> LoadRecentFilesData()
		{
			var recentSessions = new List<RecentSessions>();
			recentSessions.Add(new RecentSessions() { SerialNumber = 1, FilePath = "FirstSession.sess", LastOpened = "22-Aug-2017" });
			recentSessions.Add(new RecentSessions() { SerialNumber = 2, FilePath = "SecondSession.sess", LastOpened = "22-Aug-2017" });
			recentSessions.Add(new RecentSessions() { SerialNumber = 3, FilePath = "ThirdSession.sess", LastOpened = "22-Aug-2017" });
			recentSessions.Add(new RecentSessions() { SerialNumber = 4, FilePath = "FourthSession.sess", LastOpened = "22-Aug-2017" });
			recentSessions.Add(new RecentSessions() { SerialNumber = 5, FilePath = "FifthSession.sess", LastOpened = "22-Aug-2017" });
			return recentSessions;
		}
		private void TurnOnOffControls()
		{
			if (_controlsFlag)
			{
				ABCSessionBtn.Visibility = Visibility.Visible;
				MushraSessionBtn.Visibility = Visibility.Visible;
				ABXSessionBtn.Visibility = Visibility.Visible;
			}
			else
			{
				ABCSessionBtn.Visibility = Visibility.Hidden;
				MushraSessionBtn.Visibility = Visibility.Hidden;
				ABXSessionBtn.Visibility = Visibility.Hidden;
			}

		}
		private void NewSession_Click(object sender, RoutedEventArgs e)
		{
			_controlsFlag = true;
			TurnOnOffControls();
		}

		private void OpenSession_Click(object sender, RoutedEventArgs e)
		{
			_controlsFlag = false;
			TurnOnOffControls();
		}

		private void Dashboard_Unloaded(object sender, RoutedEventArgs e)
		{
		}

		private void Dashboard_Loaded(object sender, RoutedEventArgs e)
		{
			dataGrid.ItemsSource = _recentSessions;
		}
			

		private void ABCSessionBtn_Click(object sender, RoutedEventArgs e)
		{
			Window window = new Window
			{
				Title = "ABC SESSION WINDOW",
				Content = new ABCSessionWindow()
			};
		
			window.SetWindowProperties();
			if (window.ShowDialog() == true)
			{
				var parentWindow = (Window)this.Parent;
				parentWindow.DialogResult = true;
				parentWindow.Close();
			}
			else
			{
				var parentWindow = (Window)this.Parent;
				parentWindow.DialogResult = false;
				parentWindow.Close();
			}
		}

	}
	
}
