using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Specialized;
using System.Windows.Controls.Primitives;
using SLEP.Models;
using SLEP.UIModule.Handlers;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using SLEP.UIModule.Handlers.Enums;
using SLEP.Logger;
using System.Windows.Media;
//using System.Drawing;


namespace SLEP.UIModule.Views
{
	/// <summary>
	/// Interaction logic for FivePointScale.xaml
	/// </summary>
	[Export("ScoringScales")]
	[PartCreationPolicy(CreationPolicy.Shared)]
	public partial class ScoringScales : UserControl
	{
		private NameValueCollection _scaleSection;
		private double _score;
		private double _value;
		private static int _eventCount = 0;
        private ABCTestDetailsModel _abcModelList;
		private bool _sliderChanged = false;
		public static event EventHandler<bool> ScoringSliderEvent;
		
		public ScoringScales()
		{
			InitializeComponent();

			_abcModelList = ABCTestDetailsModel.GetInstance;
			LoadScaleSection();
			PopulateScaleParameters();

			ABCTestView.AddScore += ABCTestView_AddScore;
			ABCTestView.TestModeOnOff += TestModeOnOff;
			ABCSessionWindow.SessionCreated += ABCSessionWindow_SessionCreated;
			ABCSessionWindow.SessionClosed += ABCSessionWindow_SessionClosed;
		}

		private void TestModeOnOff(object sender, bool e)
		{
		
			BBtn.IsChecked = false;
			CBtn.IsChecked = false;

			ResetScale();
		}

		private void ABCSessionWindow_SessionClosed(object sender, ABCTestDetailsModel e)
		{
			ScoreSlider.IsEnabled = false;
			BBtn.IsEnabled = false;
			CBtn.IsEnabled = false;
			BBtn.IsChecked = false;
			CBtn.IsChecked = false;
            _sliderChanged = false;
            ResetScale();
		}

		private void ResetScale()
		{
			_sliderChanged = false;
			
			if (ScoreSlider.Minimum < 0)
			{
				ScoreSlider.Value = 0;
			}
			else
			{
				ScoreSlider.Value = ScoreSlider.Minimum;
			}
			
		}
		private void ABCSessionWindow_SessionCreated(object sender, EventArgs e)
		{
			LoadScaleSection();
			PopulateScaleParameters();
			ScoreSlider.IsEnabled = true;
			BBtn.IsEnabled = true;
			CBtn.IsEnabled = true;
		}

		private void ScoreChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			
			if (BBtn.IsChecked == false && CBtn.IsChecked == false)
			{
				if (ScoreSlider.Value != ScoreSlider.Minimum && ScoreSlider.Value != 0.0d)
				{
					ResetScale();
					MessageBox.Show("Please select any one of the stimulus to score against", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
					Log.Write(LogLevel.Warning, "File Name: ScoringScales.xaml.cs, Method Name: Score Changed, Line Number: 51");
					Log.Write(LogLevel.Warning, "Please select any one of the stimulus to score against");
					return;
				}
			}
			else
			{

				_sliderChanged = true;
			}

			_value = ScoreSlider.Value;			
		}

		private void ABCTestView_AddScore(object sender, bool testModeFlag)
		{
			if (testModeFlag)
			{
				ScoringSliderEvent?.Invoke(sender, _sliderChanged);
				if (_sliderChanged == false)
				{
					return;
				}

				_score = _value;
				_abcModelList.Score = _score;
				if (_eventCount < _abcModelList.trials.Count)
				{
					_abcModelList.ScoredStimulus = BBtn.IsChecked == true ? "B" : "C";
				}
				else
				{
					_eventCount = 0;
				}
				_eventCount++;
			}
			else
			{
				_abcModelList.Score = 0.0;
			}
			
			BBtn.IsChecked = false;
			CBtn.IsChecked = false;
            ResetScale();

        }
		private void LoadScaleSection()
		{
			_eventCount = 0;
			var scaleObject = ScaleFactory.GetScalesObject(_abcModelList.Index, _abcModelList.ScaleName);			
			_scaleSection = scaleObject.LoadScale();
		}

		private void PopulateScaleParameters()
		{
			if (SliderContent.Comment != null)
			{
				SliderContent.Comment.Clear();
				SliderContent.Score.Clear();
			}

			SliderContent.Comment = new List<string>();
			SliderContent.Score = new List<string>();

			if(_abcModelList.Index == (short)ScaleType.DifferenceModeScale)
			{
				SettingsForDifferenceScale();
			}
			else
			{
				ScaleSettings();
			}
			
					
		}
		private void PopulateSliderContent()
		{
			foreach (var key in _scaleSection.AllKeys)
			{
				SliderContent.Comment.Add(key);
				SliderContent.Score.Add(_scaleSection.GetValues(key).FirstOrDefault());
			}
		}
		
		private void SettingsForDifferenceScale()
		{
			ScoreSlider.Maximum = 15.0;
			ScoreSlider.Minimum = -15.0;
			ScoreSlider.TickFrequency = 3;
			ScoreSlider.TickPlacement = TickPlacement.Both;
			PopulateSliderContent();
		}

		private void ScaleSettings()
		{			
			ScoreSlider.Maximum = Convert.ToDouble(_scaleSection[_scaleSection.Count - 1]);
			ScoreSlider.Minimum = Convert.ToDouble(_scaleSection[0]);
			ScoreSlider.TickFrequency = (Convert.ToDouble(_scaleSection[1]) - Convert.ToDouble(_scaleSection[0]));
			ScoreSlider.TickPlacement = TickPlacement.Both;
			PopulateSliderContent();
		}
		
	}
}
