using SLEP.Logger;
using SLEP.Models;
using SLEP.UIModule.Handlers;
using SLEP.UIModule.Handlers.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace SLEP.UIModule.Views.MUSHRATestViews
{
	/// <summary>
	/// Interaction logic for MushraTestScoreView.xaml
	/// </summary>
	[Export("MUSHRATestScoreView")]
	[PartCreationPolicy(CreationPolicy.Shared)]
	public partial class MushraTestScoreView : UserControl
	{
		private const int MAXSTIMULUS = 12;
		private int _clickedButton;
		private IDictionary _buttonNames = new Dictionary<int, Button>();
		private List<Slider> _sliders = new List<Slider>();
		public static event EventHandler<int> MushraClickedButton;
		private NameValueCollection _scaleSection;
		private MushraTestDetailsModel _mushraModelList;
		private event EventHandler SliderChanged;
		private string _sliderName = "";
		private double _score = 0.0;
		private int _numberOfActiveStimulus;
		//private bool _resetFlag = false;
		private int _previousClickedButton = 0;
		public static event EventHandler<bool> ScoringSliderEvent;
		private IList<double> _scoreList = new List<double> { 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d };
		private IList<string> _stlimulusNames = new List<string> {"A","B","C","D","E","F","G","H","I","J","K","L"};
		private IList<bool> _stimulusPlayed = new List<bool> { false, false, false, false, false, false, false, false, false, false, false, false };
		private IList<bool> _stimulusScored = new List<bool> { false, false, false, false, false, false, false, false, false, false, false, false };

		public MushraTestScoreView()
		{
			InitializeComponent();
			try
			{
				_buttonNames.Add(1, ABtn);
				_buttonNames.Add(2, BBtn);
				_buttonNames.Add(3, CBtn);
				_buttonNames.Add(4, DBtn);
				_buttonNames.Add(5, EBtn);
				_buttonNames.Add(6, FBtn);
				_buttonNames.Add(7, GBtn);
				_buttonNames.Add(8, HBtn);
				_buttonNames.Add(9, IBtn);
				_buttonNames.Add(10,JBtn);
				_buttonNames.Add(11,KBtn);
				_buttonNames.Add(12,LBtn);

				_sliders.Add(sliderA);
				_sliders.Add(sliderB);
				_sliders.Add(sliderC);
				_sliders.Add(sliderD);
				_sliders.Add(sliderE);
				_sliders.Add(sliderF);
				_sliders.Add(sliderG);
				_sliders.Add(sliderH);
				_sliders.Add(sliderI);
				_sliders.Add(sliderJ);
				_sliders.Add(sliderK);
				_sliders.Add(sliderL);

				_mushraModelList = MushraTestDetailsModel.GetInstance;
				_mushraModelList.Score = new List<double>();
				LoadScaleSection();
				PopulateScaleParameters();
                MUSHRASessionWindow.SessionCreated += OnMushraSessionCreated;
				MushraTestTransportControlsView.MushraNextTrial += OnMushraNextTrial;
                MUSHRASessionWindow.SessionClosed += OnMushraSessionClosed;
				MushraTestTransportControlsView.AddScore += MushraTestTransportControlsView_AddScore;
				MushraTestTransportControlsView.MushraTestModeOnOff += MushraTestModeOnOff;
				SliderChanged += MushraTestScoreView_SliderChanged;
			
			}
			catch (Exception) { }
		}

		private void MushraTestModeOnOff(object sender, bool e)
		{
			_mushraModelList.Score.Clear();

			for (var index = 0; index < MAXSTIMULUS; index++)
			{
				_stimulusPlayed[index] = false;
				_stimulusScored[index] = false;
			}


			ResetButtons();
			ResetScales();
			_clickedButton = 0;
		}

		private void OnMushraSessionCreated(object sender, MushraTestDetailsModel e)
        {
			//_resetFlag = false;			
			LoadScaleSection();
            PopulateScaleParameters();
			_clickedButton = 0;
			RefBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1AB3D6"));
		}

        private void OnMushraSessionClosed(object sender, MushraTestDetailsModel e)
        {

			_sliders.ForEach(slider => slider.IsEnabled = false);

			for (var count = 1; count <= MAXSTIMULUS; count++)
			{
				var button = (Button)_buttonNames[count];
				button.IsEnabled = false;
			}
			RefBtn.IsEnabled = false;
			ResetButtons();
			ResetScales();
		}


		int index = 0;
		private void MushraTestScoreView_SliderChanged(object sender, EventArgs e)
		{
		
			if (index < _scoreList.Count)
			{
				_scoreList[index] = _score;
				_stimulusScored[index] = true;
			}
			_sliderName = "";
		}
		

		private void OnMushraNextTrial(object sender, int enabledButtonCount)
		{

			_scoreList = new List<double> { 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d, 0.0d };

			for (var count = 1; count <= MAXSTIMULUS; count++)
            {
                if (count <= enabledButtonCount)
                {
                    var button = (Button)_buttonNames[count];
                    button.IsEnabled = true;
                     
                    _sliders[count - 1].IsEnabled = true;
                }
                else
                {
                    var button = (Button)_buttonNames[count];
                    button.IsEnabled = false;
                    _sliders[count - 1].IsEnabled = false;

                }
			}
			RefBtn.IsEnabled = true;
			RefBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1AB3D6"));
			_clickedButton = 0;
			index = 0;
			_numberOfActiveStimulus = enabledButtonCount;
			
		}

		public void OnMushraStimulus_Clicked(object sender, RoutedEventArgs e)
		{
			var buttonObject = e.OriginalSource as Button;
						
			_previousClickedButton = _clickedButton;
			
			switch (buttonObject.Name)
			{
				case "RefBtn":
					_clickedButton = 0;
					RefBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1AB3D6"));
					break;
				case "ABtn":
					_clickedButton = 1;
					((Button)_buttonNames[_clickedButton]).Content = FindResource("APlaying");
					_stimulusPlayed[0] = MushraTestTransportControlsView.IsPlaying ? true: false;
					break;
				case "BBtn":
					_clickedButton = 2;
					((Button)_buttonNames[_clickedButton]).Content = FindResource("BPlaying");
					_stimulusPlayed[1] = MushraTestTransportControlsView.IsPlaying ? true : false;
					break;
				case "CBtn":
					_clickedButton = 3;
					((Button)_buttonNames[_clickedButton]).Content = FindResource("CPlaying");
					_stimulusPlayed[2] = MushraTestTransportControlsView.IsPlaying ? true : false;
					break;
				case "DBtn":
					_clickedButton = 4;
					((Button)_buttonNames[_clickedButton]).Content = FindResource("DPlaying");
					_stimulusPlayed[3] = MushraTestTransportControlsView.IsPlaying ? true : false;
					break;
				case "EBtn":
					_clickedButton = 5;
					((Button)_buttonNames[_clickedButton]).Content = FindResource("EPlaying");
					_stimulusPlayed[4] = MushraTestTransportControlsView.IsPlaying ? true : false;
					break;
				case "FBtn":
					_clickedButton = 6;
					((Button)_buttonNames[_clickedButton]).Content = FindResource("FPlaying");
					_stimulusPlayed[5] = MushraTestTransportControlsView.IsPlaying ? true : false;
					break;
				case "GBtn":
					_clickedButton = 7;
					((Button)_buttonNames[_clickedButton]).Content = FindResource("GPlaying");
					_stimulusPlayed[6] = MushraTestTransportControlsView.IsPlaying ? true : false;
					break;
				case "HBtn":
					_clickedButton = 8;
					((Button)_buttonNames[_clickedButton]).Content = FindResource("HPlaying");
					_stimulusPlayed[7] = MushraTestTransportControlsView.IsPlaying ? true : false;
					break;
				case "IBtn":
					_clickedButton = 9;
					((Button)_buttonNames[_clickedButton]).Content = FindResource("IPlaying");
					_stimulusPlayed[8] = MushraTestTransportControlsView.IsPlaying ? true : false;
					break;
				case "JBtn":
					_clickedButton = 10;
					((Button)_buttonNames[_clickedButton]).Content = FindResource("JPlaying");
					_stimulusPlayed[9] = MushraTestTransportControlsView.IsPlaying ? true : false;
					break;
				case "KBtn":
					_clickedButton = 11;
					((Button)_buttonNames[_clickedButton]).Content = FindResource("KPlaying");
					_stimulusPlayed[10] = MushraTestTransportControlsView.IsPlaying ? true : false;
					break;
				case "LBtn":
					_clickedButton = 12;
					((Button)_buttonNames[_clickedButton]).Content = FindResource("LPlaying");
					_stimulusPlayed[11] = MushraTestTransportControlsView.IsPlaying ? true : false;
					break;
			}

			if (_previousClickedButton != _clickedButton)
			{
				switch (_previousClickedButton)
				{
					case 0:
						RefBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5D6A77"));
						break;
					case 1:
						((Button)_buttonNames[_previousClickedButton]).Content = FindResource("A");
						break;
					case 2:
						((Button)_buttonNames[_previousClickedButton]).Content = FindResource("B");
						break;
					case 3:
						((Button)_buttonNames[_previousClickedButton]).Content = FindResource("C");
						break;
					case 4:
						((Button)_buttonNames[_previousClickedButton]).Content = FindResource("D");
						break;
					case 5:
						((Button)_buttonNames[_previousClickedButton]).Content = FindResource("E");
						break;
					case 6:
						((Button)_buttonNames[_previousClickedButton]).Content = FindResource("F");
						break;
					case 7:
						((Button)_buttonNames[_previousClickedButton]).Content = FindResource("G");
						break;
					case 8:
						((Button)_buttonNames[_previousClickedButton]).Content = FindResource("H");
						break;
					case 9:
						((Button)_buttonNames[_previousClickedButton]).Content = FindResource("I");
						break;
					case 10:
						((Button)_buttonNames[_previousClickedButton]).Content = FindResource("J");
						break;
					case 11:
						((Button)_buttonNames[_previousClickedButton]).Content = FindResource("K");
						break;
					case 12:
						((Button)_buttonNames[_previousClickedButton]).Content = FindResource("L");
						break;
				}
			}
			MushraClickedButton?.Invoke(sender, _clickedButton);
		}
		string _previousSliderName = "";
		private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			var sliderObject = e.OriginalSource as Slider;
			if (_sliderName == "")
			{
				_sliderName = sliderObject.Name;				
			}
			else if (_sliderName != sliderObject.Name)
			{
				_previousSliderName = sliderObject.Name.ToString();
				SliderChanged?.Invoke(sender, e);
			}
				
			index = _sliders.ToList().IndexOf(sliderObject);
			_score = sliderObject.Value;
			
		}

		private bool CheckAllSamplesPlayedAndScored()
		{
			bool allTrialsPlayed = false;
			bool allTrialsScored = false;
			var count = 0;
			_stimulusPlayed.ToList().ForEach(flag =>
			{
				if (flag == true)
				{
					count++;
				}
			});
			allTrialsPlayed = (count == _numberOfActiveStimulus) ? true : false;
			count = 0;
			_stimulusScored.ToList().ForEach(flag =>
			{
				if (flag == true)
				{
					count++;
				}
			});
			allTrialsScored = (count == _numberOfActiveStimulus) ? true : false;

			return (allTrialsPlayed & allTrialsScored);
		}

		private void MushraTestTransportControlsView_AddScore(object sender, bool testModeFlag)
		{
			if (testModeFlag)
			{
				EventArgs e = null;
				_sliderName = "";

				SliderChanged?.Invoke(sender, e);

				bool flag = CheckAllSamplesPlayedAndScored();
				ScoringSliderEvent?.Invoke(sender, flag);
				if (flag == false)
				{
					MessageBox.Show("Can't iterate to next trial unless the current sample is played and scored.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
					return;
				}
				
			}
			_mushraModelList.Score.Clear();
			_mushraModelList.Score = _scoreList.ToList();
			_mushraModelList.StimulusName = _stlimulusNames.ToList();
			_scoreList.Clear();
			ResetScales();
			ResetButtons();
			//_resetFlag = false;

			for (var index = 0; index < _numberOfActiveStimulus; index++)
			{
				_stimulusPlayed[index] = false;
				_stimulusScored[index] = false;
			}
		}

		private void ResetScales()
		{
			
			for (var count = 0; count < _numberOfActiveStimulus; count++)
			{
				var scaleMin = (_sliders[count]).Minimum;
				if (scaleMin < 0)
				{
					(_sliders[count]).Value = 0;
				}
				else
				{
					(_sliders[count]).Value = scaleMin;
				}
			}
		}

		private void ResetButtons()
		{
			((Button)_buttonNames[1]).Content = FindResource("A");
			((Button)_buttonNames[2]).Content = FindResource("B");
			((Button)_buttonNames[3]).Content = FindResource("C");
			((Button)_buttonNames[4]).Content = FindResource("D");
			((Button)_buttonNames[5]).Content = FindResource("E");
			((Button)_buttonNames[6]).Content = FindResource("F");
			((Button)_buttonNames[7]).Content = FindResource("G");
			((Button)_buttonNames[8]).Content = FindResource("H");
			((Button)_buttonNames[9]).Content = FindResource("I");
			((Button)_buttonNames[10]).Content = FindResource("J");
			((Button)_buttonNames[11]).Content = FindResource("K");
			((Button)_buttonNames[12]).Content = FindResource("L");
			RefBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5D6A77"));
		}
		private void LoadScaleSection()
		{
			//_eventCount = 0;
			var scaleObject = ScaleFactory.GetScalesObject(_mushraModelList.Index, _mushraModelList.ScaleName);
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
			switch (_mushraModelList.Index)
			{
				case (short)ScaleType.FivePointMushraScale:
					SettingsForFivePointMushScale();
					Log.Write(LogLevel.Info, "Constructing FivePointMushra Scale....");
					break;
				case (short)ScaleType.FivePointBS1116Scale:
					SettingsForFivePointBS1116Scale();
					Log.Write(LogLevel.Info, "Constructing FivePointBS1116 Scale....");
					break;
				case (short)ScaleType.SevenPointBS562Scale:
					SettingsForSevenPointScale();
					Log.Write(LogLevel.Info, "Constructing SevenPoint Scale....");
					break;
				case (short)ScaleType.DifferenceModeScale:
					SettingsForDifferenceScale();
					Log.Write(LogLevel.Info, "Constructing DifferenceMode Scale....");
					break;
				default:
					SettingsForCustomScale();
					break;
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
		private void SettingsForFivePointMushScale()
		{
			for (var count = 0; count < MAXSTIMULUS; count++)
			{
				((Slider)_sliders[count]).Maximum = 100;
				((Slider)_sliders[count]).Minimum = 0;
				((Slider)_sliders[count]).TickFrequency = 20;
				((Slider)_sliders[count]).IsSnapToTickEnabled = false;
				((Slider)_sliders[count]).TickPlacement = TickPlacement.Both;
			}
		
			PopulateSliderContent();
		}

		private void SettingsForFivePointBS1116Scale()
		{
			for (var count = 0; count < MAXSTIMULUS; count++)
			{
				((Slider)_sliders[count]).Maximum = 5;
				((Slider)_sliders[count]).Minimum = 1;
				((Slider)_sliders[count]).TickFrequency = 1;
				((Slider)_sliders[count]).IsSnapToTickEnabled = false;
				((Slider)_sliders[count]).TickPlacement = TickPlacement.Both;
			}

			PopulateSliderContent();
		}

		private void SettingsForSevenPointScale()
		{
			for (var count = 0; count < MAXSTIMULUS; count++)
			{
				((Slider)_sliders[count]).Maximum = 3;
				((Slider)_sliders[count]).Minimum = -3;
				((Slider)_sliders[count]).TickFrequency = 1;
				((Slider)_sliders[count]).IsSnapToTickEnabled = false;
				((Slider)_sliders[count]).TickPlacement = TickPlacement.Both;
			}

			PopulateSliderContent();
		}

		private void SettingsForDifferenceScale()
		{
			for (var count = 0; count < MAXSTIMULUS; count++)
			{
				((Slider)_sliders[count]).Maximum = 15;
				((Slider)_sliders[count]).Minimum = -15;
				((Slider)_sliders[count]).TickFrequency = 3;
				((Slider)_sliders[count]).IsSnapToTickEnabled = false;
				((Slider)_sliders[count]).TickPlacement = TickPlacement.Both;
			}

			PopulateSliderContent();
		}

		private void SettingsForCustomScale()
		{

			sliderA.Maximum = Convert.ToDouble(_scaleSection[_scaleSection.Count - 1]);
			sliderA.Minimum = Convert.ToDouble(_scaleSection[0]);
			sliderA.TickFrequency = (Convert.ToDouble(_scaleSection[1]) - Convert.ToDouble(_scaleSection[0]));
			sliderA.TickPlacement = TickPlacement.Both;
			PopulateSliderContent();
		}
	}
}
