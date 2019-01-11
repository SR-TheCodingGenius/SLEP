using SLEP.Models;
using SLEP.ViewModels;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Media;
using System.Linq;
using SLEP.UIModule.Views.MUSHRATestViews;
using SLEP.UIModule.Menu;
using SLEP.UIModule.Views;
using System.Windows.Input;
// ReSharper disable All

namespace SLEP.Views
{
	/// <summary>
	/// Interaction logic for ShellWindow.xaml
	/// </summary>
	/// 
	[Export]
	public partial class ShellWindow : Window
	{
		private ABCTestDetailsModel _abcModelList;
		private MushraTestDetailsModel _mushraModelList;
		private double _dataGridWidth = 0.0;
		//private bool _abcAuditionModeFlag = false;
		//private bool _mushraAuditionModeFlag = false;
		private int _abcAuditionModeFlag = -1;
		private int _mushraAuditionModeFlag = -1;
		private int _trialCount = 0;

		[ImportingConstructor]
		public ShellWindow()
		{
			InitializeComponent();
			_abcModelList = ABCTestDetailsModel.GetInstance;
			_abcModelList.trials = new List<ABCSessionModel>();

			_mushraModelList = MushraTestDetailsModel.GetInstance;
			_mushraModelList.trials = new List<MushraSessionModel>();

			ABCSessionWindow.DisplaySession += ABCSessionWindow_DisplaySession;
			ABCSessionWindow.SessionClosed += ABCSessionWindow_SessionClosed;

			MUSHRASessionWindow.DisplaySession += MUSHRASessionWindow_DisplaySession;
			MUSHRASessionWindow.SessionClosed += MUSHRASessionWindow_SessionClosed;

			ABCTestView.TestModeOnOff += ABCTestModeOnOff;
			MushraTestTransportControlsView.MushraTestModeOnOff += MushraTestModeOnOff;

			ABCTestView.AddScore += ABCAddScore;
			//MushraTestTransportControlsView.AddScore += MushraAddScore;
			_dataGridWidth = SessGrid.Width;

		}

		private void MushraAddScore(object sender, bool e)
		{
			
			_trialCount++;
			SessGrid.SelectionChanged -= SessGrid_SelectionChanged;
			if (_trialCount == _mushraModelList.trials.Count())
			{
				_trialCount = 0;
			}
			SessGrid.SelectedIndex = _trialCount;
			SessGrid.SelectionChanged += SessGrid_SelectionChanged;
		}

		private void ABCAddScore(object sender, bool e)
		{
			_trialCount++;
			SessGrid.SelectionChanged -= SessGrid_SelectionChanged;
			if (_trialCount == _abcModelList.trials.Count())
			{
				_trialCount = 0;
			}
			SessGrid.SelectedIndex = _trialCount;
			SessGrid.SelectionChanged += SessGrid_SelectionChanged;
		}

		private void MushraTestModeOnOff(object sender, bool e)
		{
			if (e)
			{
				MainMenu.IsEnabled = false;
				_mushraAuditionModeFlag = 1;
                SessGrid.Items.Refresh();
            }
			else
			{
				MainMenu.IsEnabled = true;
				_mushraAuditionModeFlag = 0;
			}
		}

		private void ABCSessionWindow_DisplaySession(object sender, ABCTestDetailsModel e)
		{
			SessGrid.ItemsSource = null;
			SessGrid.Items.Clear();
			SessGrid.Width = _dataGridWidth;
			var titleText = string.Format("SLEP - ABC Session ({0})", e.SessionFileName);
			this.Title = titleText;
			//SessGrid.SelectedIndex = 0;
			_abcModelList.trials.All(trials =>
			{
				var trial = new TrialList();

				trial.TrialNumber = trials.TrialNumber;
				trial.TrialName = trials.TrialName;

				SessGrid.Items.Add(trial);
				SessGrid.Columns[0].Width = 450;
				SessGrid.Columns[1].Width = 450;
				return true;
			});

		}
		private void MUSHRASessionWindow_DisplaySession(object sender, MushraTestDetailsModel e)
		{
			SessGrid.ItemsSource = null;
			SessGrid.Items.Clear();


			var trialList = new List<TrialList>();
			var titleText = string.Format("SLEP - MUSHRA Session ({0})", e.SessionFileName);
			this.Title = titleText;
			//SessGrid.SelectedIndex = 0;
          
			_mushraModelList.trials.All(trials =>
			{
				var trial = new TrialList();

				trial.TrialNumber = trials.TrialNumber;
				trial.TrialName = trials.TrialName;

				SessGrid.Items.Add(trial);
				SessGrid.Width = 1200;
				SessGrid.Columns[0].Width = 500;
				SessGrid.Columns[1].Width = 690;               


                return true;
			});

		}

		private void ABCSessionWindow_SessionClosed(object sender, System.EventArgs e)
		{
			var titleText = "SLEP";
			Title = titleText;
			_abcAuditionModeFlag = -1;
			SessGrid.ItemsSource = null;
			SessGrid.Items.Clear();
		}

		private void MUSHRASessionWindow_SessionClosed(object sender, MushraTestDetailsModel e)
		{
			var titleText = "SLEP";
			Title = titleText;			
			_mushraAuditionModeFlag = -1;
			SessGrid.ItemsSource = null;
			SessGrid.Items.Clear();
		}

		private void ABCTestModeOnOff(object sender, bool e)
		{
			if (e)
			{
				MainMenu.IsEnabled = false;
				_abcAuditionModeFlag = 1;
                SessGrid.Items.Refresh();
            }
			else
			{
				MainMenu.IsEnabled = true;
				_abcAuditionModeFlag = 0;
			}
		}



		[Import]
		private ShellWindowViewModel ViewModel
		{
			get { return this.DataContext as ShellWindowViewModel; }
			set { this.DataContext = value; }
		}

		private void SlepWindow_Loaded(object sender, RoutedEventArgs e)
		{
			this.ResizeMode = ResizeMode.NoResize;
			this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
			this.SizeToContent = SizeToContent.WidthAndHeight;
		}

		// File Menu Events
		private void OnABCSessionClicked(object sender, RoutedEventArgs e)
		{
			var fileMenuObject = new FileMenuViewObject();
			fileMenuObject.AbcNewSession();          
		}

		private void OnMushraSessionClicked(object sender, RoutedEventArgs e)
		{
			var fileMenuObject = new FileMenuViewObject();
			fileMenuObject.MushraNewSession();
		}

		private void OnOpenSessionClicked(object sender, RoutedEventArgs e)
		{
			var fileMenuObject = new FileMenuViewObject();
			fileMenuObject.OpenSession(sender, e);
		}

		private void OnCloseSessionClicked(object sender, RoutedEventArgs e)
		{
			var fileMenuObject = new FileMenuViewObject();
			fileMenuObject.CloseSession(sender, e);
		}
		private void OnExitClicked(object sender, RoutedEventArgs e)
		{
			var fileMenuObject = new FileMenuViewObject();
			fileMenuObject.ExitApplication();
		}
		//

		// Option Menu Events
		private void OnSettingsClicked(object sender, RoutedEventArgs e)
		{
			var optionsMenuObject = new OptionsMenuViewObject();
			optionsMenuObject.OnSettingsClicked();
		}
		private void OnCustomScalesClicked(object sender, RoutedEventArgs e)
		{
			var optionsMenuObject = new OptionsMenuViewObject();
			optionsMenuObject.OnCustomScalesClicked();
		}

		private void OnEditSessionClicked(object sender, RoutedEventArgs e)
		{
			var fileMenuObject = new FileMenuViewObject();
			fileMenuObject.EditSession();
		}

		private void OnViewScoreSheetClicked(object sender, RoutedEventArgs e)
		{
			var viewMenuObject = new ViewMenuViewObject();
			//viewMenuObject.LoadScoreWindow();
			viewMenuObject.ViewScoreSheet();
		}

		private void OnScoreMergeClicked(object sender, RoutedEventArgs e)
		{
			var toolsMenuObject = new ToolsMenuViewObject();
			toolsMenuObject.ViewScoreMergeWindow();
		}

		
		private void OnAboutClicked(object sender, RoutedEventArgs e)
		{
			var aboutMunuObject = new AboutMenuViewObject();
			aboutMunuObject.AboutDialog();
		}

		private void SessGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			if (SessGrid.SelectedIndex < 0)
			{
				return;
			}
		
			var selectedTrial = SessGrid.SelectedIndex;
			_trialCount = selectedTrial - 1;
			
			if (_abcAuditionModeFlag == 0)
			{
				ABCTestView.LoadSelectedTrial(selectedTrial);
			}
			else if (_mushraAuditionModeFlag == 0)
			{
				MushraTestTransportControlsView.LoadSelectedTrial(selectedTrial);
			}
			
		}

		private void SessGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (_mushraAuditionModeFlag == 1 || _abcAuditionModeFlag == 1)
			{
				e.Handled = true;
			}
		}
	}

	public static class WindowSettings
	{
		public static void SetWindowProperties(this Window window)
		{
			window.ResizeMode = ResizeMode.NoResize;
			
			var color = (Color)ColorConverter.ConvertFromString("#E4EEF4");
			window.Background = new SolidColorBrush(color);
			window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
		}
	}


}
