using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.ComponentModel;
using System.Threading;
using System.Windows.Media;
using System.ComponentModel.Composition;
using SLEP.Models;
using SLEP.SessionAndScore.Logger;
using SLEP.SessionAndScore.Logger.Enums;
using SLEP.SessionAndScore.Logger.Handlers;
using SLEP.WaveDisplay;
using SLEP.Audio;
using SLEP.NAudio.Enums;
using SLEP.Logger;
using SLEP.UIModule.Handlers;
using SLEP.UIModule.Menu;

namespace SLEP.UIModule.Views
{
    /// <summary>
    /// Interaction logic for ABCTestView.xaml
    /// </summary>

    [Export("ABCTestView")]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class ABCTestView : UserControl, INotifyPropertyChanged
    {
        private IList<WavePlayer> _doAudioOperations;
		private IList<WavePlayer> _newlistOfWavePlayers;
		private IAudioService _audioObject;

        private IWaveformPlayer _waveDisplay;
        private MicroTimer _microTimer = new MicroTimer();

        private string _refFile;
        private string _sampleFile;
        private int _clickedButton;
        private int _current = -1;
        private int _next = -1;
        private float _repeatStart;
        private float _repeatEnd;
        private bool _isPlaying = false;
        private bool _loopFalg = false;
        private bool _sessCreatedFlag = false;
        private bool _isCanvasCleared = true;
        private int _trialCount;
        private bool _testModeFlag;
        private List<ABCSessionModel> abcSessionEnumerator;
        private AudioSettingsModel _settingsObject;
        private ScorerDetails _scorerDetails;
        private ABCTestDetailsModel _abcTestDetailsModel;
        private static event EventHandler<int> LoadSelectedTrialEvent;
        private const float FadeOutTime = 50.0f;
        private int _sessionIndex = 0;
        private bool _trailPlayed = false;
        private bool _scoringSliderFlag = false;
		//private bool _mouseClicked = false;
        ISessionAndScoreLogger _scoreLogger;


        public static event EventHandler<bool> AddScore;
        public event PropertyChangedEventHandler PropertyChanged;
        public static event EventHandler<bool> TestModeOnOff;

        #region ABCTestView Constructor
        public ABCTestView()
        {
            InitializeComponent();

            _scoreLogger = LogFactory.GetLoggerObject((byte)LoggerType.ScoreLogger);
            ABCSessionWindow.SessionCreated += ABCSessionWindow_SessionCreated;
            ABCSessionWindow.SessionClosed += ABCSessionWindow_SessionClosed;
            ScoringScales.ScoringSliderEvent += ScoringSliderEvent;

            LoadSelectedTrialEvent += OnLoadSelectedTrialEvent;
            _waveDisplay = WaveFormPlayer.Instance;
            _settingsObject = AudioSettingsModel.GetInstance;
            _scorerDetails = ScorerDetails.GetInstance;
            this.Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
            var image = (Image)FindResource("NoLoop");
            image.Height = 50;
            LoopBtn.Content = image;

        }
        #endregion
        #region Custom Event Handlers
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

        private void ABCSessionWindow_SessionCreated(object sender, ABCTestDetailsModel e)
        {
            _abcTestDetailsModel = e;

            ClearState();


            LoopBtn.Content = (Image)FindResource("NoLoop");

            ConfigureTimer();


            abcSessionEnumerator = _abcTestDetailsModel?.trials?.ToList();
            _trialCount = abcSessionEnumerator.Count();

            _sessCreatedFlag = ConstructABCTestComponents();

            if (_isCanvasCleared)
            {
                WaveFormDisplay.ConstructWaveFormControl();
                _isCanvasCleared = false;
            }
            TestModeOnOff?.Invoke(this, false);
            if (_sessCreatedFlag)
            {
                _settingsObject.SessionCreatedFlag = true;
                PlayEnabled(true);
				HighlightPlayingAudio(0);
				Log.Write(LogLevel.Info, "The ABC Session has been loaded Sucessfully!!!");
            }
			else
			{
				using (var obj = new FileMenuViewObject())
				{
					obj.CloseSession();
				}
				
			}
            
        }
        private void ABCSessionWindow_SessionClosed(object sender, EventArgs e)
        {
            if (_isPlaying)
            {
                _audioObject?.FadeOut(_doAudioOperations[_current], 20);
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
            _waveDisplay.ChannelPosition = 0.0d;
            WaveFormDisplay.RepeatRegionHeight = 0;
            WaveFormDisplay.RepeatRegionWidth = 0;
            SelectEnd = 0.0f;
            SelectBegin = 0.0f;
            _waveDisplay.SelectionBegin = TimeSpan.Zero;
            _waveDisplay.SelectionEnd = TimeSpan.Zero;
            _isCanvasCleared = WaveFormDisplay.ClearCanvas();
            LoopBtn.Content = FindResource("NoLoop");
            PlayEnabled(false);
            HighlightPlayingAudio(-1);
            _settingsObject.SessionCreatedFlag = false;
            trialName.Content = "";
            _microTimer.Stop();

            _clickedButton = 0;
            _loopFalg = false;
            _sessionIndex = 0;
            _isPlaying = false;
            _sessCreatedFlag = false;
        }
        private void OnLoadSelectedTrialEvent(object sender, int selectedTrial)
        {
            _sessionIndex = selectedTrial;

            ResetForNextTrial();
            DisposeAudioComponents();
            AddScore?.Invoke(sender, _testModeFlag);
            ConstructABCTestComponents();
        }
        private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
        {
            DisposeAudioComponents();

            if (!_microTimer.StopAndWait(1000))
            {
                _microTimer.Abort();
            }
        }
        private void ScoringSliderEvent(object sender, bool e)
        {
            _scoringSliderFlag = e;
        }
        #endregion
        #region Private Methods of ABCTestView class		
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
        private void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
        private void ConfigureTimer()
        {
            _microTimer.Interval = 500;
            _microTimer.MicroTimerElapsed += OnTimerElapsed;
        }

        private bool ConstructABCTestComponents()
        {
            if (_sessionIndex == abcSessionEnumerator.Count)
            {
                _sessionIndex = 0;
                return false;
            }

            var currentAbcSessionObject = abcSessionEnumerator.ElementAtOrDefault(_sessionIndex);
            if (currentAbcSessionObject.Reference == "" || currentAbcSessionObject.Sample == "")
            {
                System.Windows.Forms.MessageBox.Show("Reference and Sample files should not be empty!!!");
                return false;
            }

            _refFile = currentAbcSessionObject.Reference;
            _sampleFile = currentAbcSessionObject.Sample;

            _audioObject = AudioFactory.GetAudioObject(_settingsObject.OutputDriver, _settingsObject.AsioDriver, _settingsObject.ExclusiveMode);

            var latency = 0;
            if (_settingsObject.OutputDriver == 0)
            {
                if (_settingsObject.Latency < 100)
                {
                    latency = _settingsObject.Latency * _settingsObject.NumberOfBuffers;
                }
                else
                {
                    latency = _settingsObject.Latency;
                }
            }

			if(! _audioObject.IntializeNAudioLibrary(latency))
			{
				return false;
			}

            _doAudioOperations = new List<WavePlayer>();
			_newlistOfWavePlayers = new List<WavePlayer>();

			if (_doAudioOperations == null)
            {
                throw new NullReferenceException("The WavePlayer instance is not initialized!!!!");
            }
            _doAudioOperations.Add(new WavePlayer(_refFile));
			_newlistOfWavePlayers.Add(new WavePlayer(_refFile));

            if (currentAbcSessionObject.HiddenReference.Trim() == "B")
            {
                _doAudioOperations.Add(new WavePlayer(_refFile));
                _doAudioOperations.Add(new WavePlayer(_sampleFile));
				_newlistOfWavePlayers.Add(new WavePlayer(_refFile));
				_newlistOfWavePlayers.Add(new WavePlayer(_sampleFile));
			}
            else
            {
                _doAudioOperations.Add(new WavePlayer(_sampleFile));
                _doAudioOperations.Add(new WavePlayer(_refFile));
				_newlistOfWavePlayers.Add(new WavePlayer(_sampleFile));
				_newlistOfWavePlayers.Add(new WavePlayer(_refFile));
			}

            trialName.Content = string.Format("Trial {0} of {1}, Trial Name - {2}", currentAbcSessionObject.TrialNumber, _trialCount, currentAbcSessionObject.TrialName);

            _doAudioOperations.All(audioChannels => { audioChannels.SetVolumeStream(0); return true; });
			_newlistOfWavePlayers.All(audioChannels => { audioChannels.SetVolumeStream(0); return true; });

            _audioObject.MixWaveProviders(_doAudioOperations);

            WaveFormPlayer.Instance.OpenFile(_refFile);

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
                string currentTimeStamp = currenttime.Minutes + ":" + currenttime.Seconds + ":" + currenttime.Milliseconds;

                if (_isPlaying)
                {
                    _waveDisplay.ChannelPosition = currenttime.TotalSeconds;
                }

                StartText.Text = currentTimeStamp;

                PlayInSelectedRegion();

				if (_loopFalg == false && SelectBegin <= 0)
				{
					ResetAtMaximum();
				}

				StlimusSwitcher();
            });

        }
		bool _regionChangedFlag = false;
        private void SetSelections()
        {
            if (_isCanvasCleared)
                return;
			if(Math.Abs(SelectBegin - (float)_waveDisplay.SelectionBegin.TotalSeconds) > 0.0 || Math.Abs(SelectEnd - (float)_waveDisplay.SelectionEnd.TotalSeconds) > 0.0)
			{
				_regionChangedFlag = true;
			}
			else
			{
				_regionChangedFlag = false;
			}
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
            }

            if (SelectEnd > _audioObject.GetTotalDuration().TotalSeconds)
            {
                SelectEnd = (float)_audioObject.GetTotalDuration().TotalSeconds;
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
                    if (_loopFalg && DelayFadeOutSampleProvider._triStateFlag == 1)
					{
						_audioObject.SetCurrentTime(TimeSpan.FromSeconds(SelectBegin));
						_audioObject.CrossFadeAtEnds(_doAudioOperations[_current]);
						_audioObject.CopyofSampleProvider(_newlistOfWavePlayers[_current], SelectBegin);
						_fadeinFlag = true;
						DelayFadeOutSampleProvider._triStateFlag = 0;
					}

                    if (!_loopFalg && DelayFadeOutSampleProvider._triStateFlag == 1)
                    {
						_microTimer.Stop();
						PlayEnabled(true);
						_fadeoutFlag = true;
						Thread.Sleep(500);
						_audioObject.StopAudio();
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

			//if ((_audioObject.GetTotalDuration().TotalMilliseconds - _audioObject.GetCurrentTime().TotalMilliseconds <= 200.0) && _current != -1)
			//{
			//	_audioObject.FadeOut(_doAudioOperations[_current], 20);
			//}

			if (_waveDisplay.ChannelPosition >= (_waveDisplay.ChannelLength - temporaryFix))
            {
                StartText.Text = "0:0:0";
                if (_audioObject != null)
                {
                    //Thread.Sleep(500);
                    _audioObject.StopAudio();
                    PlayEnabled(true);
                    _isPlaying = false;
                    _microTimer.Stop();

                    _waveDisplay.ChannelPosition = 0;
                    Log.Write(LogLevel.Info, "Audio components got reset to the start postion, audio player reached the end of the file");
                }
            }
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

        private void HighlightPlayingAudio(int buttonClicked)
        {
            switch (buttonClicked)
            {
                case 0:
                    ABtn.Content = FindResource("APlaying");
                    BBtn.Content = FindResource("B");
                    CBtn.Content = FindResource("C");
                    break;
                case 1:
                    ABtn.Content = FindResource("A");
                    BBtn.Content = FindResource("BPlaying");
                    CBtn.Content = FindResource("C");
                    break;
                case 2:
                    ABtn.Content = FindResource("A");
                    BBtn.Content = FindResource("B");
                    CBtn.Content = FindResource("CPlaying");
                    break;
                default:
                    ABtn.Content = FindResource("A");
                    BBtn.Content = FindResource("B");
                    CBtn.Content = FindResource("C");
                    _current = -1;
                    _next = -1;
                    break;
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
                MessageBox.Show("Session has ended \n To continue either reload the session again or switch between the modes", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                throw new NullReferenceException("The Audio Object is not Intialized yet!!!!");
            }

            HighlightPlayingAudio(buttonClicked);
			SetSelections();

			if (!_isPlaying)
			{
				_current = buttonClicked;
				
				if(_testModeFlag)
				{
					if (_current == 0)
					{
						_aFlag = true;						
					}
					else if(_current == 1)
					{
						_bFlag = true;
					}
					else if (_current == 2)
					{
						_cFlag = true;
					}
				}

				_audioObject.FadeIn(_doAudioOperations[_current], _settingsObject.CrossfadeTime);
				_fadeinFlag = true;
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
                if (_current == 0)
                {
                    _aFlag = _testModeFlag ? true : false;
                }
            }
			//_audioObject.CopyofSampleProvider(_newlistOfWavePlayers[buttonClicked], SelectBegin, SelectEnd, _settingsObject.CrossfadeTime);
		}

        private void PauseAudio()
        {
            if (_isPlaying)
            {
                _isPlaying = false;
                _microTimer.Stop();

                _audioObject.FadeoutOnMouseClicksAndPause(_doAudioOperations[_current], _settingsObject.CrossfadeTime);
                Thread.Sleep(500);
                _audioObject.PauseAudio();


                Log.Write(LogLevel.Info, "All Audio files are paused!!!!");
            }
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
            _microTimer.Stop();

            LoopBtn.Content = FindResource("NoLoop");
            PlayEnabled(true);
            WaveFormDisplay.RepeatRegionHeight = 0;
            WaveFormDisplay.RepeatRegionWidth = 0;
            HighlightPlayingAudio(0);

            _isPlaying = false;
            _clickedButton = 0;
            _loopFalg = false;
            _trailPlayed = false;
            _scoringSliderFlag = false;
        }

        private void LogScores()
        {
            Log.Write(LogLevel.Info, "Intiated LogScores Method to log the scores of the trials in the current session");
            _scoreLogger.FileName = _scorerDetails.ScoreFileName;
            _scoreLogger.WriteFile();
        }

        private void ResetAllInAuditionMode()
        {
            TestModeBtn.Content = "In Audition Mode";
            TestModeBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5D6A77"));

            if (_audioObject.GetPlayBackState() != PlayBackState.Stopped)
            {
                _audioObject.StopAudio();
            }

            _microTimer.Stop();
            TestModeOnOff?.Invoke(this, false);

            _waveDisplay.ChannelPosition = 0;
            _sessionIndex = 0;
            _clickedButton = 0;
            _loopFalg = false;
            _testModeFlag = false;
            WaveFormDisplay.RepeatRegionHeight = 0;
            WaveFormDisplay.RepeatRegionWidth = 0;

            PlayEnabled(true);
            DisposeAudioComponents();
            ConstructABCTestComponents();
            HighlightPlayingAudio(0);
            LoopBtn.Content = (Image)FindResource("NoLoop");
        }

        private void ResetAllInTestMode()
        {
            TestModeBtn.Content = "In Test Mode";
            TestModeBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#02C4A4"));

            if (_audioObject.GetPlayBackState() != PlayBackState.Stopped)
            {
                _audioObject.StopAudio();
            }
            _microTimer.Stop();
            TestModeOnOff?.Invoke(this, true);

            _waveDisplay.ChannelPosition = 0;
            _sessionIndex = 0;
            _clickedButton = 0;
            _loopFalg = false;
            _testModeFlag = true;

            WaveFormDisplay.RepeatRegionHeight = 0;
            WaveFormDisplay.RepeatRegionWidth = 0;

            PlayEnabled(true);
            DisposeAudioComponents();
            ConstructABCTestComponents();
            HighlightPlayingAudio(0);
            LoopBtn.Content = (Image)FindResource("NoLoop");

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
                _scoreLogger.InitializeLogger(_abcTestDetailsModel);
                _scoreLogger.WriteHeaders();
                return true;
            }
            else
            {
                return false;
            }

        }
        #endregion
        #region Public Methods and Properties of ABCTestView Class
        public static void LoadSelectedTrial(int trialNumber)
        {
            var sender = new object();
            LoadSelectedTrialEvent.Invoke(sender, trialNumber);
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
        #endregion
        #region Event Handlers for in built events
        bool _aFlag = false;
        bool _bFlag = false;
        bool _cFlag = false;

        private void ABtn_Click(object sender, RoutedEventArgs e)
        {
            _clickedButton = 0;
            if (_sessCreatedFlag)
            {
                HighlightPlayingAudio(_clickedButton);

            }
            if (_isPlaying)
            {
                PlayClickedAudio(_clickedButton);
                _aFlag = _testModeFlag ? true : false;
                Log.Write(LogLevel.Info, "Playing Stimulus - A Audio file!!!");
            }

        }
        private void BBtn_Click(object sender, RoutedEventArgs e)
        {
            _clickedButton = 1;
            if (_sessCreatedFlag)
            {
                HighlightPlayingAudio(_clickedButton);
            }
            if (_isPlaying)
            {
                PlayClickedAudio(_clickedButton);
                _bFlag = _testModeFlag ? true : false;
                Log.Write(LogLevel.Info, "Playing Stimulus - B Audio file!!!");
            }
        }
        private void CBtn_Click(object sender, RoutedEventArgs e)
        {
            _clickedButton = 2;
            if (_sessCreatedFlag)
            {
                HighlightPlayingAudio(_clickedButton);
            }
            if (_isPlaying)
            {
                PlayClickedAudio(_clickedButton);
                _cFlag = _testModeFlag ? true : false;
                Log.Write(LogLevel.Info, "Playing Stimulus - C Audio file!!!");
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
                    //SetSelections();
                }

            }

        }
        private void PlayBtn_Click(object sender, RoutedEventArgs e)
        {

            if (_sessCreatedFlag == false)
            {
                return;
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
                PlayClickedAudio(_clickedButton);
            }
            else if (PlayBtn.Content == (Image)FindResource("PauseEnabled"))
            {
                PauseAudio();
                PlayEnabled(true);
            }

        }
        bool _allTrialsCompleted = false;
        private void trialBtn_Click(object sender, RoutedEventArgs e)
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
                if (_aFlag && _bFlag && _cFlag)
                {
                    _trailPlayed = true;
                }
                else
                {
                    _trailPlayed = false;
                }
                if (_trailPlayed == false || _scoringSliderFlag == false)
                {
                    PauseAudio();
                    PlayEnabled(true);
                    MessageBox.Show("Can't iterate to next trial unless the current sample is played and scored.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                else
                {
                    _aFlag = false;
                    _bFlag = false;
                    _cFlag = false;
                    _scoringSliderFlag = false;
                }

            }
            //AddScore?.Invoke(sender, _testModeFlag);
            ResetForNextTrial();

            if (_testModeFlag)
            {
                LogScores();
            }

            DisposeAudioComponents();

            var hasMoreTrials = ConstructABCTestComponents();


            if (hasMoreTrials == false)
            {
                _allTrialsCompleted = true;


                if (_testModeFlag)
                {
                    MessageBox.Show("The Test is Complete");
                    _scoreLogger.CloseFile();
                    ResetAllInAuditionMode();
                    Log.Write(LogLevel.Info, "The Test is Complete");
                    _allTrialsCompleted = false;
                }
                else
                {
                    _sessionIndex = 0;
                    ConstructABCTestComponents();
                }
                _waveDisplay.ChannelPosition = 0.0;
            }
            else
            {
                _allTrialsCompleted = false;
            }
		}
				
		private void OnLeftButtonUp(object sender, RoutedEventArgs e)
		{
			
			SetSelections();
			if (_isPlaying)
			{
				
				
				_audioObject.FadeoutOnMouseClicksAndPause(_doAudioOperations[_current], _settingsObject.CrossfadeTime);
				Thread.Sleep(150);
				if (_audioObject != null)
				{
					_audioObject.SetCurrentTime(TimeSpan.FromSeconds(_waveDisplay.ChannelPosition));
					//_audioObject.CopyofSampleProvider(_newlistOfWavePlayers[_current], SelectBegin, SelectEnd, _settingsObject.CrossfadeTime);
				//	if (!_loopFalg)
					{
						_audioObject.UpdateSelectionTimesonMouseClicks((float)_waveDisplay.ChannelPosition, SelectEnd);
					}
					_audioObject.FadeIn(_doAudioOperations[_current],_settingsObject.CrossfadeTime);
					//_mouseClicked = true;
				}
			}

			var timeStamp = _waveDisplay.SelectionEnd;
            var timeStamptext = timeStamp.Minutes + ":" + timeStamp.Seconds + ":" + timeStamp.Milliseconds;
            StopText.Text = timeStamptext;

            timeStamp = _waveDisplay.SelectionBegin;
            timeStamptext = timeStamp.Minutes + ":" + timeStamp.Seconds + ":" + timeStamp.Milliseconds;
            StartText.Text = timeStamptext;
        }
        private void TestModeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_sessCreatedFlag == false)
            {
                return;
            }

            if (TestModeBtn.Content.ToString() == "In Test Mode")
            {
                if (!_allTrialsCompleted)
                {
                    if (MessageBox.Show("Are you sure!!! you want to exit the Test Mode???", "Info", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
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
                        TestModeOnOff?.Invoke(sender, false);
                        return;
                    }
                    ResetAllInTestMode();
                }
                else
                {
                    TestModeOnOff?.Invoke(sender, false);
                }
            }

        }
        #endregion

    }

}
