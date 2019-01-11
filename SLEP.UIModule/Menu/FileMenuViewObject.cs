using SLEP.Models;
using SLEP.UIModule.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using SLEP.Logger;
using SLEP.Extensions.Handlers;

namespace SLEP.UIModule.Menu
{

	public partial class FileMenuViewObject : IDisposable
	{
		private ABCTestDetailsModel _abcModelList;
		private MushraTestDetailsModel _mushraModelList;
		private ScorerDetails _scorerDetails;
		private int _countSampleTags = 0;
		private string _fileName = "";
		private static bool _abcFlag = false;
		private static bool _mushraFlag = false;
        public FileMenuViewObject()
		{
			
			_abcModelList = ABCTestDetailsModel.GetInstance;
			_mushraModelList = MushraTestDetailsModel.GetInstance;
			_scorerDetails = ScorerDetails.GetInstance;
			ABCTestView.TestModeOnOff += ABCTest_TestModeOnOff;
		}

		public void AbcNewSession()
		{
			
			Window window = new Window
			{
				Title = string.Format("ABC SESSION WINDOW \t {0}", _fileName),
				Content = new ABCSessionWindow(_fileName)
			};

			window.SetWindowProperties();
			if (window.ShowDialog() == true)
			{
				_abcFlag = true;
				_mushraFlag = false;				
			}

		}

		public int SampleTags {get { return _countSampleTags; } }

		public void MushraNewSession()
		{
			
			Window window = new Window
			{
				Title = string.Format("MUSHRA SESSION WINDOW \t {0}", _fileName),
				Content = new MUSHRASessionWindow(_fileName)
			};

			window.SetWindowProperties();
			if(window.ShowDialog() == true)
			{
				_abcFlag = false;
				_mushraFlag = true;
				
			}

		}

		public void OpenSession(object sender, RoutedEventArgs e)
		{
			//var openFileDialog = new OpenFileDialog();
			//openFileDialog.Filter = "Session Files|*.sess";
			var openFileDialog = new Microsoft.Win32.OpenFileDialog();
			var fileName = openFileDialog.BrowseFileToOpen("Session Files|*.sess");

			if (!string.IsNullOrEmpty(fileName))
			{
				FlushPreviousSessionAndScoreDetails();
				if (_abcFlag)
				{
					ABCSessionWindow.InvokeSessionClosed(sender, _abcModelList);
				}
				else if (_mushraFlag)
				{
					MUSHRASessionWindow.InvokeSessionClosed(sender, _mushraModelList);
				}
				var msg = string.Format("Opening an Exisiting Session from the file {0}", fileName);
				Log.Write(LogLevel.Info, msg);
				if (ReadSessionFile(fileName))
				{
                    if (_countSampleTags > 1)
                    {
						_mushraFlag = true;
						_abcFlag = false;
						MUSHRASessionWindow.LoadMushraView();
                        MUSHRASessionWindow.InvokeEvents(sender, _mushraModelList);
                    }
                    else
					{
						_mushraFlag = false;
						_abcFlag = true;
						ABCSessionWindow.LoadAbcView();
						ABCSessionWindow.InvokeEvents(sender, _abcModelList);
                    }
				}
				else
				{
					System.Windows.Forms.MessageBox.Show("The Session file is corrupted!!!! \n Or the audio files path is incorrect", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
			}

		}

		public void EditSession()
		{
			_fileName = _abcModelList.SessionFileName == "" ? _mushraModelList.SessionFileName : _abcModelList.SessionFileName;

			if (_fileName == "")
			{
				//var openFileDialog = new OpenFileDialog();
				//openFileDialog.Filter = "Session Files|*.sess";
				var openFileDialog = new Microsoft.Win32.OpenFileDialog();
				var fileName = openFileDialog.BrowseFileToOpen("Session Files|*.sess");
				if (!string.IsNullOrEmpty(fileName))
				{
					_fileName = fileName;
				}
				else
				{
					return;
				}
			}

			var msg = string.Format("Editing an Exisiting Session for the file {0} ", _fileName);
			Log.Write(LogLevel.Info, msg);
			FlushPreviousSessionAndScoreDetails();
			if (ReadSessionFile(_fileName))
			{
					
				
				if (_countSampleTags > 1)
				{
					_mushraFlag = true;
					_abcFlag = false;
					MushraNewSession();
				}
				else
				{
					_mushraFlag = false;
					_abcFlag = true;
					AbcNewSession();					
				}
					
			}
			else
			{
				System.Windows.Forms.MessageBox.Show("The Session file is corrupted!!!! \n Or the audio files path is incorrect", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			

		}

		public bool ReadSessionFile(string fileName)
		{
			string line = "";
			var count = 0;
			var searchTexts = 0;
			var numOfRecords = 0;
			bool samplFileFlag = false;
			bool refFileFlag = false;
			bool sampleTagFlag = false;
			var lineCounter = 1;
			
			
			try
			{
				using (var streamReader = new StreamReader(fileName))
				{
					while ((line = streamReader.ReadLine()) != null)
					{
						if ((line.ToLower()).Contains("number_of_trials:"))
						{
							var num = line.Split('=');
							numOfRecords = Convert.ToInt16(num[1]);
						}
						
						if ((line.ToLower()).Contains("sample:="))
						{
							sampleTagFlag = true;
							if (lineCounter - _countSampleTags == 1)
							{
								_countSampleTags++;
							}
							
						}
						if(sampleTagFlag)
						{
							lineCounter++;
						}
					}
					sampleTagFlag = false;
				}
				

				if (numOfRecords == 0)
				{
					return false;
				}
				using (var streamReader = new StreamReader(fileName))
				{
					var list = new List<string>();
					var lineCount = 0;
					var scaleIndex = 0;
					var scaleName = "";

					if (_countSampleTags > 1)
					{
                        _mushraModelList.trials?.Clear();
                        for (var count1 = 0; count1 < numOfRecords; count1++)
						{
							_mushraModelList.trials.Add(new MushraSessionModel());
							_mushraModelList.trials[count1].Sample.Clear();
						}
					}
					else
					{
                        _abcModelList.trials?.Clear();

                        for (var count1 = 0; count1 < numOfRecords; count1++)
						{
							_abcModelList.trials.Add(new ABCSessionModel());
						
						}
					}
					while ((line = streamReader.ReadLine()) != null)
					{
						if ((line.ToLower()).Contains("scale_index:"))
						{
							var index = line.Split('=');
							scaleIndex = Convert.ToInt16(index[1]);
						}

						if ((line.ToLower()).Contains("scale_selected:"))
						{
							var scale = line.Split('=');
							scaleName = scale[1];
						}

						if ((line.ToLower()).Contains("hidden_reference:"))
						{
							var hiddenref = line.Split('=');
							if (_countSampleTags > 1)
							{
								_mushraModelList.trials[count].HiddenReference = hiddenref[1];

							}
							else
							{
								_abcModelList.trials[count].HiddenReference = hiddenref[1];
							}
							searchTexts++;
						}

						if ((line.ToLower()).Contains("reference:"))
						{
							var refFilePath = line.Split('=');
							if (File.Exists(refFilePath[1]))
							{
								if (_countSampleTags > 1)
								{
									if (count < numOfRecords && (refFilePath[0].ToLower()).Contains("sample") == false)
									{
										_mushraModelList.trials[count].Reference = refFilePath[1];
									}
								}
								else
								{
									if (count < numOfRecords)
									{
										_abcModelList.trials[count].Reference = refFilePath[1];
									}
								}
								searchTexts++;
								refFileFlag = true;
							}
							else
							{
								refFileFlag = false;
							}
						}

						if ((line.ToLower()).Contains("sample:"))
						{
							var sampFilePath = line.Split('=');
							if (_countSampleTags > 1)
							{
								if (File.Exists(sampFilePath[1]) && sampFilePath[1] != "")
								{
									if (count < numOfRecords)
									{
										_mushraModelList.trials[count].Sample.Add(sampFilePath[1]);
										Log.Write(LogLevel.Info, string.Format("For Trial Number -> {0} the sample File Added -> {1}", 
											_mushraModelList.trials[count].TrialNumber, sampFilePath[1] ));
									}
									samplFileFlag = true;
								}
								else
								{
									samplFileFlag = false;
								}
							}
							else
							{
								if (File.Exists(sampFilePath[1]))
								{
									if (count < numOfRecords)
									{
										_abcModelList.trials[count].Sample = sampFilePath[1];
									}
									samplFileFlag = true;
								}
								else
								{
									samplFileFlag = false;
								}
							}
							searchTexts++;
						}

						if ((line.ToLower()).Contains("trial_name:"))
						{
							var trialName = line.Split('=');
							if (_countSampleTags > 1)
							{
								_mushraModelList.trials[count].TrialName = trialName[1];
								_mushraModelList.trials[count].TrialNumber = count + 1;
								searchTexts++;
								count++;
							}
							else
							{
								_abcModelList.trials[count].TrialName = trialName[1];
								_abcModelList.trials[count].TrialNumber = count + 1;
								searchTexts++;
								count++;
							}
						}

						if ((line.ToLower()).Contains("file" + lineCount))
						{
							var mediaPoolPath = line.Split('=');
							list.Add(mediaPoolPath[1]);
							lineCount++;
							//count++;
						}


					}

					if (_countSampleTags > 1)
					{
						MUSHRASessionWindow._mediaPool = list;
						_mushraModelList.Index = (short)scaleIndex;
						_mushraModelList.ScaleName = scaleName;
						_mushraModelList.SessionFileName = fileName;
						_mushraModelList.TestName = "MUSHRA Test";
					}
					else
					{
						ABCSessionWindow._mediaPool = list;
						_abcModelList.Index = (short)scaleIndex;
						_abcModelList.ScaleName = scaleName;
						_abcModelList.SessionFileName = fileName;
						_abcModelList.TestName = "Triple Stimulus Test / Hidden Reference / ABC Test";
					}
				}

				return (refFileFlag & samplFileFlag);
			}
			catch(Exception ex)
			{
				System.Windows.Forms.MessageBox.Show("The Reading of the Session File is Failed, The file may be corrupted", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
				throw new Exception("The Reading of the Session File is Failed, The file may be corrupted");
			}
		}

		private void FlushPreviousSessionAndScoreDetails()
		{
			if (_abcFlag)
			{
				ABCSessionWindow._mediaPool?.Clear();
				_abcModelList.trials?.Clear();
				_abcModelList.Score = 0.0;
				_abcModelList.Index = 0;
				_abcModelList.ScaleName = "";
				_abcModelList.SessionFileName = "";
				_abcModelList.TestName = "";
				_scorerDetails.Age = 0;
				_scorerDetails.FirstName = "";
				_scorerDetails.LastName = "";
				_scorerDetails.Notes = "";
				_scorerDetails.ScoreFileName = "";
				_scorerDetails.Sex = "";
			}
			else if(_mushraFlag)
			{
				MUSHRASessionWindow._mediaPool?.Clear();
				_mushraModelList.trials?.Clear();
				_mushraModelList.Score?.Clear();
				_mushraModelList.Index = 0;
				_mushraModelList.ScaleName = "";
				_mushraModelList.SessionFileName = "";
				_mushraModelList.TestName = "";
				_scorerDetails.Age = 0;
				_scorerDetails.FirstName = "";
				_scorerDetails.LastName = "";
				_scorerDetails.Notes = "";
				_scorerDetails.ScoreFileName = "";
				_scorerDetails.Sex = "";
			}

			Log.Write(LogLevel.Info, "The Current Session and Score Details are Flushed Off!!!");
		}

		public void CloseSession()
		{
			FlushPreviousSessionAndScoreDetails();

			if (_abcFlag)
			{
				ABCSessionWindow.InvokeSessionClosed(this, _abcModelList);
			}
			else if (_mushraFlag)
			{
				MUSHRASessionWindow.InvokeSessionClosed(this, _mushraModelList);
			}

			if (_abcFlag || _mushraFlag)
			{
				System.Windows.Forms.MessageBox.Show("The Session is Closed!!!!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
				Log.Write(LogLevel.Info, "The Current Session is Closed!!!");
				_abcFlag = false;
				_mushraFlag = false;
			}
		}
		public void CloseSession(object sender, RoutedEventArgs e)
		{
			CloseSession();
		}

		private void ABCTest_TestModeOnOff(object sender, bool e)
		{
			//if (e == true)
			//{
			//	FileMenu.IsEnabled = false;
			//}
			//else
			//{
			//	FileMenu.IsEnabled = true;
			//}
		}

		public void ExitApplication()
		{
			Log.Write(LogLevel.Info, "Exiting the SLEP Application");
			System.Windows.Application.Current.Shutdown();
		}

		public void Dispose()
		{
			
		}
	}
}

