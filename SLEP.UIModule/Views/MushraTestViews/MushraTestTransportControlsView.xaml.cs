using SLEP.Audio;
using SLEP.Logger;
using SLEP.Models;
using SLEP.NAudio.Enums;
using SLEP.SessionAndScore.Logger;
using SLEP.SessionAndScore.Logger.Enums;
using SLEP.SessionAndScore.Logger.Handlers;
using SLEP.UIModule.Handlers;
using SLEP.UIModule.Menu;
using SLEP.WaveDisplay;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SLEP.UIModule.Views.MUSHRATestViews
{
	/// <summary>
	/// Interaction logic for MushraTestAudioView.xaml
	/// </summary>
	[Export("MushraTestTransportControlsView")]
	[PartCreationPolicy(CreationPolicy.Shared)]
	public partial class MushraTestTransportControlsView : UserControl, INotifyPropertyChanged
	{
		private static bool _isPlaying = false;
		private bool _isCanvasCleared = false;
		private bool _sessCreatedFlag = false;
		private bool _loopFalg = false;
		private bool _testModeFlag;
		private int _current = -1;
		private int _next = -1;
		private float _repeatStart;
		private float _repeatEnd;
		private IList<WavePlayer> _doAudioOperations;
		private IList<WavePlayer> _newlistOfWavePlayers = null;
		private IAudioService _audioObject;
		private IWaveformPlayer _waveDisplay;
		private AudioSettingsModel _settingsObject;
		private List<MushraSessionModel> _mushraSessionEnumerator;
		private MushraTestDetailsModel _mushraTestDetailsModel;
		private MicroTimer _microTimer = new MicroTimer();
		public event PropertyChangedEventHandler PropertyChanged;
		public static event EventHandler<int> MushraNextTrial;
		public static event EventHandler<bool> MushraTestModeOnOff;
		private ScorerDetails _scorerDetails;
		ISessionAndScoreLogger _scoreLogger;
		public static event EventHandler<bool> AddScore;
		private static event EventHandler<int> LoadSelectedTrialEvent;
		private int _mushraButtonIndex = 0;
		private int _currentTrialNumber = -1;
		private int _sessionIndex = 0;
		private bool _scoringSliderFlag = false;

		public MushraTestTransportControlsView()
		{
			_waveDisplay = WaveFormPlayer.Instance;
			_settingsObject = AudioSettingsModel.GetInstance;
			_scoreLogger = LogFactory.GetLoggerObject((byte)LoggerType.ScoreLogger);
			_scorerDetails = ScorerDetails.GetInstance;

			MUSHRASessionWindow.SessionCreated += MUSHRASessionWindow_SessionCreated;
			MUSHRASessionWindow.SessionClosed += MUSHRASessionWindow_SessionClosed;
			MushraTestScoreView.MushraClickedButton += OnMushraClickedButton;
			MushraTestScoreView.ScoringSliderEvent += ScoringSliderEvent;
			LoadSelectedTrialEvent += OnLoadSelectedTrialEvent;
			this.Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
		
			InitializeComponent();
		}

		public static bool IsPlaying { get { return _isPlaying; } }
		private void ScoringSliderEvent(object sender, bool e)
		{
			_scoringSliderFlag = e;
			if(e == false)
			{
				PlayEnabled(true);
			}
		}

		private void MUSHRASessionWindow_SessionClosed(object sender, MushraTestDetailsModel e)
		{
			if (_isPlaying)
			{
				_audioObject.FadeOut(_doAudioOperations[_current], 20);
				Thread.Sleep(500);
			}
			ClearState();
		}


		private void ClearState()
		{
			DisposeAudioComponents();
			if (_doAudioOperations != null)
			{
				_doAudioOperations.Clear();
			}
			if (_sessCreatedFlag)
			{
				_isCanvasCleared = WaveFormDisplay.ClearCanvas();				
			}

			_waveDisplay.ChannelPosition = 0.0d;
			_isPlaying = false;
			_sessCreatedFlag = false;
			trialName.Content = "";
		
			PlayEnabled(false);
			_settingsObject.SessionCreatedFlag = false;
			LoopBtn.Content = (Image)FindResource("NoLoop");
			_microTimer.Stop();
			_sessionIndex = 0;
			_loopFalg = false;
			WaveFormDisplay.RepeatRegionHeight = 0;
			WaveFormDisplay.RepeatRegionWidth = 0;
			_current = -1;
			_next = -1;
			StartText.Clear();
			StopText.Clear();
		}
		private void MUSHRASessionWindow_SessionCreated(object sender, MushraTestDetailsModel e)
		{
			_mushraTestDetailsModel = e;
			ClearState();

			_mushraSessionEnumerator = _mushraTestDetailsModel.trials.ToList();

			_sessCreatedFlag = ConstructMushraComponents();			
			
			ConfigureTimer();
			
			if (_isCanvasCleared)
			{
				WaveFormDisplay.ConstructWaveFormControl();
				_isCanvasCleared = false;
			}

			if (_sessCreatedFlag)
			{
				MushraTestModeOnOff?.Invoke(this, false);
				PlayEnabled(true);
				_settingsObject.SessionCreatedFlag = true;
				Log.Write(LogLevel.Info, "The MUSHRA Session has been loaded Sucessfully!!!");
				
			}
			else
			{
				using (var obj = new FileMenuViewObject())
				{
					MessageBox.Show("Audio Hardware not Connected/Installed", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					obj.CloseSession();
				}

			}
		}

		public static void LoadSelectedTrial(int trialNumber)
		{
			var sender = new object();
			LoadSelectedTrialEvent.Invoke(sender, trialNumber);
		}
		
		private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
		{
			DisposeAudioComponents();

			if (!_microTimer.StopAndWait(1000))
			{
				_microTimer.Abort();
			}
		}

		private void DisposeAudioComponents()
		{
			if (_doAudioOperations == null)
			{
				return;
			}
			_doAudioOperations.All(audioObj =>
			{
				_audioObject.StopAudio();
				_isPlaying = false;
				audioObj.Dispose(); return true;
			});
			_audioObject.Dispose();
		
		}

		private void ConfigureTimer()
		{
			_microTimer.Interval = 10000;
			_microTimer.MicroTimerElapsed += OnTimerElapsed;
		}

		private bool IsRegionExists
		{
			get
			{
				if (WaveFormDisplay.RepeatRegionHeight > 0.0 && WaveFormDisplay.RepeatRegionWidth > 0.0)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		public float SelectBegin
		{
			get { return _repeatStart; }
			set
			{
				_repeatStart = value;
				NotifyPropertyChanged("SelectionBegin");
			}
		}

		public float SelectEnd
		{
			get { return _repeatEnd; }
			set
			{
				_repeatEnd = value;
				NotifyPropertyChanged("SelectionEnd");
			}
		}

		private void NotifyPropertyChanged(String info)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
		}

		private bool ConstructMushraComponents()
		{						
			if (_sessionIndex == _mushraSessionEnumerator.Count)
			{
				_sessionIndex = 0;
				return false;
			}
			var currentMushraSessionObject = _mushraSessionEnumerator.ElementAtOrDefault(_sessionIndex);
			_currentTrialNumber = currentMushraSessionObject.TrialNumber;
			var count = 0;

			currentMushraSessionObject.Sample.All(sampleName =>
			{
				if(sampleName != "")
				{
					count++;
					return true;
				}
				else
				{
					return false;
				}
			});

			MushraNextTrial?.Invoke(this, count);

			if (currentMushraSessionObject.Reference == "" || currentMushraSessionObject.Sample.Count() == 0)
			{
				System.Windows.Forms.MessageBox.Show("Reference and Sample files should not be empty!!!");
				return false;
			}

			_audioObject = AudioFactory.GetAudioObject(_settingsObject.OutputDriver, _settingsObject.AsioDriver, _settingsObject.ExclusiveMode);

			if (!_audioObject.IntializeNAudioLibrary(_settingsObject.Latency))
			{
				return false;
			}
			
			_doAudioOperations = new List<WavePlayer>();
			_newlistOfWavePlayers = new List<WavePlayer>();

			if (_doAudioOperations == null)
			{
				throw new NullReferenceException("The WavePlayer instance is not initialized!!!!");
			}

			_doAudioOperations.Add(new WavePlayer(currentMushraSessionObject.Reference));
			_newlistOfWavePlayers.Add(new WavePlayer(currentMushraSessionObject.Reference));
			currentMushraSessionObject.Sample.All(sample =>	
			{
				if (sample != "")
				{
					_doAudioOperations.Add(new WavePlayer(sample));
					_newlistOfWavePlayers.Add(new WavePlayer(sample));
				}
				return true;
				
			});

			trialName.Content = string.Format("Trial {0} of {1}, Trial Name - {2}", currentMushraSessionObject.TrialNumber, 
				_mushraTestDetailsModel.trials.Count, currentMushraSessionObject.TrialName);

			_doAudioOperations.All(audioChannels =>	{	audioChannels.SetVolumeStream(0);	return true;	});
			_newlistOfWavePlayers.All(audioChannels => { audioChannels.SetVolumeStream(0); return true; });

			_audioObject.MixWaveProviders(_doAudioOperations);

			WaveFormPlayer.Instance.OpenFile(currentMushraSessionObject.Reference);
			WaveFormDisplay.RegisterSoundPlayer(_waveDisplay);

			_sessionIndex++;
			var timeStamp = _audioObject.GetTotalDuration();
			StopText.Text = string.Format("{0}:{1}:{2}", timeStamp.Minutes, timeStamp.Seconds, timeStamp.Milliseconds);
			StartText.Text = "0:0:0";
			return true;
		}

		private void OnTimerElapsed(object sender, MicroTimerEventArgs e)
		{

			Dispatcher.Invoke(() =>
			{
				if (_doAudioOperations == null)
				{
					return;
				}
				var currenttime = _audioObject.GetCurrentTime();

				if (currenttime.TotalSeconds <= SelectEnd)
				{
					string currentTimeStamp = currenttime.Minutes + ":" + currenttime.Seconds + ":" + currenttime.Milliseconds;
					StartText.Text = currentTimeStamp;
				}

				if (!_isPlaying)
				{
					if (_audioObject.GetPlayBackState() == PlayBackState.Playing)
					{
						_audioObject.StopAudio();
						_microTimer.Stop();
					}
				}

				if (_isPlaying)
				{
					_waveDisplay.ChannelPosition = currenttime.TotalSeconds;
				}
				
				PlayInSelectedRegion();

				if (_loopFalg == false && SelectBegin <= 0)
				{
					ResetAtMaximum();
				}

				StlimusSwitcher();
			});


		}
		private void PlayEnabled(bool flag)
		{
			if (flag)
			{
				var image = (Image)FindResource("PlayEnabled");
				image.Height = 50;
				PlayBtn.Content = image;
			}
			else
			{
				var image = (Image)FindResource("PlayDisabled");
				image.Height = 50;
				PlayBtn.Content = image;
			}
		}

		
		private void SetSelections()
		{
			if (_isCanvasCleared)
				return;
		
			if (IsRegionExists)
			{
				SelectEnd = (float)_waveDisplay.SelectionEnd.TotalSeconds;
				SelectBegin = (float)_waveDisplay.SelectionBegin.TotalSeconds;
			}
			else
			{
				SelectEnd = (float)_audioObject.GetTotalDuration().TotalSeconds;
				_waveDisplay.SelectionEnd = _audioObject.GetTotalDuration();
				SelectBegin = 0.0f;
			}
			if (SelectBegin < 0.0)
			{
				SelectBegin = 0.0f;
				_waveDisplay.SelectionBegin = TimeSpan.FromSeconds(0);
			}

			if (SelectEnd >= _audioObject.GetTotalDuration().TotalSeconds)
			{
				SelectEnd = (float)_audioObject.GetTotalDuration().TotalSeconds;
				_waveDisplay.SelectionEnd = TimeSpan.FromSeconds(SelectEnd);
				SelectEnd -= 0.1f;
			}

		}
		bool _fadeinFlag = true;
		bool _fadeoutFlag = true;
		private void PlayInSelectedRegion()
		{
			if (_sessCreatedFlag == false)
			{
				return;
			}

			if (SelectBegin < SelectEnd && _isPlaying)
			{
				_waveDisplay.ChannelPosition = _audioObject.GetCurrentTime().TotalSeconds;
				var fadeOutTime = (SelectEnd - _audioObject.GetCurrentTime().TotalSeconds) * 1000.0;

				if (_waveDisplay.ChannelPosition >= SelectEnd)
				{
					if (_loopFalg)
					{
						_audioObject.CrossFadeAtEnds(_doAudioOperations[_current]);
						_audioObject.SetCurrentTime(TimeSpan.FromSeconds(SelectBegin));
						_waveDisplay.ChannelPosition = SelectBegin;
						_audioObject.CopyofSampleProvider(_newlistOfWavePlayers[_current], SelectBegin);
						_fadeinFlag = true;
					}

					if (!_loopFalg)
					{
						PlayEnabled(true);
						_fadeoutFlag = true;
						var timeStamp = _waveDisplay.SelectionBegin;
						var timeStamptext = timeStamp.Minutes + ":" + timeStamp.Seconds + ":" + timeStamp.Milliseconds;
						StartText.Text = timeStamptext;
						_waveDisplay.ChannelPosition = _waveDisplay.SelectionBegin.TotalSeconds;
						DelayFadeOutSampleProvider._triStateFlag = 0;
						_isPlaying = false;

					}
				}
				else
				{
					FadeOutAtTheEndOfRegions(fadeOutTime);
				}
			}
		}

		private void FadeOutAtTheEndOfRegions(double fadeOutTime)
		{
			if (_loopFalg)
			{
				if (fadeOutTime <= 101.0)
				{
					if (_fadeinFlag)
					{
						_audioObject.CopyofSampleProvider(_newlistOfWavePlayers[_current], SelectBegin, SelectEnd, 10);
						_audioObject.FadersInOut(_doAudioOperations[_current], 10);
						_fadeinFlag = false;
					}
				}
			}
			else
			{
				if (fadeOutTime <= 200.0)
				{
					if (_fadeoutFlag)
					{
						_audioObject.FadeOut(_doAudioOperations[_current], _settingsObject.CrossfadeTime);
						_fadeoutFlag = false;
					}
				}
			}
		}

		private void ResetAtMaximum()
		{
			var temporaryFix = 0.0d;
			if (_audioObject.GetType() == typeof(AsioAudioService))
			{
				temporaryFix = 0.1;
			}
			
			if (_waveDisplay.ChannelPosition >= (_waveDisplay.ChannelLength - temporaryFix))
			{
				StartText.Text = "0:0:0";
				if (_audioObject != null)
				{
					//Thread.Sleep(500);
					PlayEnabled(true);
					_audioObject.StopAudio();
					_isPlaying = false;
					_microTimer.Stop();
					_waveDisplay.ChannelPosition = 0;
					Log.Write(LogLevel.Info, "Audio components got reset to the start postion, audio player reached the end of the file");
				}
			}
		}

		private void LoopBtn_Click(object sender, RoutedEventArgs e)
		{
			if (_sessCreatedFlag)
			{
				if (LoopBtn.Content == FindResource("Loop"))
				{
					_loopFalg = false;
					var image = (Image)FindResource("NoLoop");
					image.Height = 50;
					LoopBtn.Content = image;
					return;
				}
				else if (LoopBtn.Content == FindResource("NoLoop"))
				{

					_loopFalg = true;
					var image = (Image)FindResource("Loop");
					image.Height = 50;
					LoopBtn.Content = image;
				}
				if (_loopFalg && IsRegionExists == false)
				{
					_waveDisplay.SelectionBegin = TimeSpan.FromSeconds(0.0);
					_waveDisplay.SelectionEnd = TimeSpan.FromSeconds(_waveDisplay.ChannelLength);
				}
			}

		}
		
		private void OnLeftButtonUp(object sender, RoutedEventArgs e)
		{
			SetSelections();
			if (_isPlaying)
			{
				_audioObject.FadeoutOnMouseClicksAndPause(_doAudioOperations[_current], 10);
				Thread.Sleep(150);
				if (_audioObject != null)
				{
					_audioObject.SetCurrentTime(TimeSpan.FromSeconds(_waveDisplay.ChannelPosition));
					_audioObject.UpdateSelectionTimesonMouseClicks((float)_waveDisplay.ChannelPosition, SelectEnd);
					_audioObject.FadeIn(_doAudioOperations[_current], 10);					
				}
			}
			var timeStamp = _waveDisplay.SelectionEnd;
			var timeStamptext = timeStamp.Minutes + ":" + timeStamp.Seconds + ":" + timeStamp.Milliseconds;
			StopText.Text = timeStamptext;

			timeStamp = _waveDisplay.SelectionBegin;
			timeStamptext = timeStamp.Minutes + ":" + timeStamp.Seconds + ":" + timeStamp.Milliseconds;
			StartText.Text = timeStamptext;
		}

		private void StlimusSwitcher()
		{
			if (_next == -1)
			{
				return;
			}

			if (_current != -1 && _next != -1)
			{
				if (_current != _next)
				{
					_audioObject.FadeInOut(_doAudioOperations[_next], _doAudioOperations[_current], _settingsObject.CrossfadeTime);
					_current = _next;
					_next = -1;
				}
			}

		}


		private void PlayClickedAudio(int buttonClicked)
		{
			if (_sessCreatedFlag == false)
			{
				return;
			}
			if (_doAudioOperations == null || _audioObject._audioFileName == "")
			{
				PlayEnabled(true);
				MessageBox.Show("Session has ended \n To continue either reload the session again \n or switch between the modes", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
				throw new NullReferenceException("The Audio Object is not Intialized yet!!!!");
			}
			
			if (!_isPlaying)
			{
				_current = buttonClicked;

				if(_current == -1)
				{
					return;
				}
				_audioObject.FadeIn(_doAudioOperations[_current], 10);
				//_fadeinFlag = true;
				_doAudioOperations.All(audioChannels => { audioChannels.SetVolumeStream(0); return true; });
				_doAudioOperations[_current].SetVolumeStream(1);

				if (_audioObject != null && ((_audioObject.GetPlayBackState() == PlayBackState.Stopped) || (_audioObject.GetPlayBackState() == PlayBackState.Paused)))
				{
					_audioObject.SetCurrentTime(TimeSpan.FromSeconds(_waveDisplay.ChannelPosition));
					_audioObject.PlayAudio();

				}
				_isPlaying = true;

			}
			else
			{
				_next = buttonClicked;
			}
			//_audioObject.CopyofSampleProvider(_newlistOfWavePlayers[buttonClicked], SelectBegin, SelectEnd, _settingsObject.CrossfadeTime);
		}

		private void PauseAudio()
		{
			_audioObject.FadeoutOnMouseClicksAndPause(_doAudioOperations[_current], 10);
			Thread.Sleep(500);
			_audioObject.PauseAudio();
			_microTimer.Stop();
			_isPlaying = false;
			Log.Write(LogLevel.Info, "All Audio files are paused!!!!");
		}

		private void OnMushraClickedButton(object sender, int buttonIndex)
		{
			_mushraButtonIndex = buttonIndex;

			if (_isPlaying)
			{
				PlayClickedAudio(buttonIndex);
			}
		}
		private void PlayBtn_Click(object sender, RoutedEventArgs e)
		{
			if (_doAudioOperations == null || _audioObject == null)
			{
				PlayEnabled(true);
				MessageBox.Show("Session has ended \n To continue either reload the session again \n or switch between the modes", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
				throw new NullReferenceException("The Audio Object is not Intialized yet!!!!");
			}

			if (!_isPlaying)
			{
				_microTimer.Start();
			}

			if (PlayBtn.Content == (Image)FindResource("PlayEnabled"))
			{
				var image = (Image)FindResource("PauseEnabled");
				image.Height = 50;
				PlayBtn.Content = image;
				PlayClickedAudio(_mushraButtonIndex);
			}
			else if (PlayBtn.Content == (Image)FindResource("PauseEnabled"))
			{
				PauseAudio();
				PlayEnabled(true);
			}
		}

		bool _allTrialsCompleted = false;
		private void OnTrialsBtnClicked(object sender, RoutedEventArgs e)
		{
			if (_sessCreatedFlag == false)
			{
				return;
			}

			AddScore?.Invoke(sender, _testModeFlag);

			if (_testModeFlag)
			{
				if (_scorerDetails.ScoreFileName == "")
				{
					MessageBox.Show("No Score Sheet has found for this session!!! Please create a New Score Sheet for this Session");
					Log.Write(LogLevel.Warning, "No Score Sheet has found for this session!!! Please create a New Score Sheet for this Session");
					return;
				}
				if(_scoringSliderFlag == false)
				{
					PauseAudio();
					return;
				}
				LogScores();
			}
			ResetForNextTrial();

			DisposeAudioComponents();

			//_doAudioOperations = null;

			var endOfListflag = ConstructMushraComponents();
            
			if (endOfListflag == false)
			{
				_allTrialsCompleted = true;
			
				if (_testModeFlag)
				{
					MessageBox.Show("The Test is Complete");
					_scoreLogger.CloseFile();
					ResetAllInAuditionMode();
					Log.Write(LogLevel.Info, "The Test is Complete");
				}
				else
				{
					_sessionIndex = 0;
					ConstructMushraComponents();
				}
				_waveDisplay.ChannelPosition = 0.0;
			}
			else
			{
				_allTrialsCompleted = false;
			}
		}

		private void OnLoadSelectedTrialEvent(object sender, int selectedTrial)
		{
			_sessionIndex = selectedTrial;
			ResetForNextTrial();
			AddScore?.Invoke(sender, _testModeFlag);
			DisposeAudioComponents();
			ConstructMushraComponents();
		}

		private void ResetForNextTrial()
		{
			if (_audioObject.GetPlayBackState() != PlayBackState.Stopped)
			{
				_audioObject.FadeOut(_doAudioOperations[_current], 20);
				Thread.Sleep(500);
				_audioObject.StopAudio();				
			}
			_audioObject.SetCurrentTime(TimeSpan.Zero);
			LoopBtn.Content = FindResource("NoLoop");
			_isPlaying = false;
			PlayEnabled(true);
			WaveFormDisplay.RepeatRegionHeight = 0;
			WaveFormDisplay.RepeatRegionWidth = 0;
			_mushraButtonIndex = 0;
			_loopFalg = false;
			//_mouseClickFlag = false;
		}
		private void LogScores()
		{						
			Log.Write(LogLevel.Info, "Intiated LogScores Method to log the scores of the trials in the current session");
			_scoreLogger.FileName = _scorerDetails.ScoreFileName;
			_scoreLogger.WriteFile();
		}
		private void OnTestModeBtnClicked(object sender, RoutedEventArgs e)
		{
			if (_sessCreatedFlag == false)
			{
				return;
			}

			if (TestModeBtn.Content.ToString() == "In Test Mode")
			{
				if (!_allTrialsCompleted)
				{
					if (MessageBox.Show("Are you sure!!! you want to exit the Test Mode???", "Info", 
						MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
					{
						_scoreLogger.CloseFile();
						ResetAllInAuditionMode();
						return;
					}
				}
			}
			else if (TestModeBtn.Content.ToString() == "In Audition Mode")
			{
				
				Window window = new Window
				{
					Title = "Score Sheet Info",
					Content = new ScoreSheetInfo()
				};
				window.SizeToContent = SizeToContent.WidthAndHeight;
				window.ResizeMode = ResizeMode.NoResize;
				window.WindowStartupLocation = WindowStartupLocation.CenterScreen;

				if (window.ShowDialog() == true)
				{
                    if (SaveScoreFile() == false)
                    {
                        MushraTestModeOnOff?.Invoke(sender, false);
                        return;
                    }

                    ResetAllInTestMode();
                }
				else
				{
                    MushraTestModeOnOff?.Invoke(sender, false);
                }
			}
		}

        private void ResetAllInAuditionMode()
        {
			TestModeBtn.Content = "In Audition Mode";
			TestModeBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5D6A77"));

			if (_audioObject.GetPlayBackState() != PlayBackState.Stopped)
			{
				_audioObject.StopAudio();
			}

			_waveDisplay.ChannelPosition = 0;
			_sessionIndex = 0;
			
			//_mouseClickFlag = false;
			_loopFalg = false;
			_testModeFlag = false;

			WaveFormDisplay.RepeatRegionHeight = 0;
			WaveFormDisplay.RepeatRegionWidth = 0;

			LoopBtn.Content = (Image)FindResource("NoLoop");

			PlayEnabled(true);
			MushraTestModeOnOff?.Invoke(this, _testModeFlag);
			DisposeAudioComponents();
			ConstructMushraComponents();
		}
        private void ResetAllInTestMode()
        {

			TestModeBtn.Content = "In Test Mode";
			TestModeBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#02C4A4"));

			if (_audioObject.GetPlayBackState() != PlayBackState.Stopped)
			{
				_audioObject.StopAudio();
			}

			_waveDisplay.ChannelPosition = 0;
			_sessionIndex = 0;

			//_mouseClickFlag = false;
			_loopFalg = false;
			_testModeFlag = true;

			WaveFormDisplay.RepeatRegionHeight = 0;
			WaveFormDisplay.RepeatRegionWidth = 0;

			LoopBtn.Content = (Image)FindResource("NoLoop");
			_allTrialsCompleted = false;
			PlayEnabled(true);
			MushraTestModeOnOff?.Invoke(this, _testModeFlag);
			DisposeAudioComponents();
			ConstructMushraComponents();			
		}

		private bool SaveScoreFile()
		{
			var saveFileDialog = new System.Windows.Forms.SaveFileDialog();
			saveFileDialog.Filter = "Score Files |*.sco";
			if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				_scoreLogger.FileName = saveFileDialog.FileName;

				_scorerDetails.ScoreFileName = _scoreLogger.FileName;
				ViewScores.FileName = _scoreLogger.FileName;
				var msg = string.Format("The score file {0} is created for the current session", _scoreLogger.FileName);
				Log.Write(LogLevel.Info, msg);
				_scoreLogger.InitializeLogger(_mushraTestDetailsModel);
				_scoreLogger.WriteHeaders();
                return true;
			}
            else
            {
                return false;
            }

		}

	}
}
