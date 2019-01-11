using SLEP.Models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using System.Windows.Forms;
using SLEP.SessionAndScore.Logger;
using SLEP.SessionAndScore.Logger.Handlers;
using SLEP.SessionAndScore.Logger.Enums;
using System;
using SLEP.Logger;
using SLEP.UIModule.Menu;
using SLEP.Extensions.Handlers;
using SLEP.UIModule.Controllers;
using Microsoft.Practices.Prism.Regions;
using System.IO;
using System.Xml;

namespace SLEP.UIModule.Views
{
	/// <summary>
	/// Interaction logic for ABCSessionWindow.xaml
	/// </summary>
	public partial class ABCSessionWindow : System.Windows.Controls.UserControl
	{
		private IList<ABCSessionModel> _abcSessionList = new List<ABCSessionModel>();
		private ABCSessionModel _abcSession;
		private int _trialNumber = 0;
		private int _prevtrialNumber = 0;
		ISessionAndScoreLogger _sessionLogger;
		private bool _sessionFileFlag;
        public static IList<string> _mediaPool;
		public static event EventHandler<ABCTestDetailsModel> DisplaySession;
		public static  event EventHandler<ABCTestDetailsModel> SessionCreated;
		public static event EventHandler<ABCTestDetailsModel> SessionClosed;
        private static ABCTestDetailsModel _abcModelList;
        private Point _startPoint;
		private string _fileName;
		private bool _sessionAlreadySaved = false;
		private static bool _sessionCreatedFlag = false;
		
        public ABCSessionWindow(string fileName = "")
		{
			InitializeComponent();
			_abcSessionList = new List<ABCSessionModel>();
            _abcModelList =  ABCTestDetailsModel.GetInstance;
			_sessionLogger = LogFactory.GetLoggerObject((byte)LoggerType.SessionLogger);
			_sessionAlreadySaved = false;
			if (fileName == "")
			{
				//OpenMItem.IsEnabled = false;
				_fileName = fileName;
			}
			else
			{
				//OpenMItem.IsEnabled = true;
				_fileName = fileName;
			}
			
			ReadCustomScaleElements();
			InitializeEditOrOpenSession();
			_abcModelList.trials.Clear();
			
		}

		private void InitializeEditOrOpenSession()
		{
			if (_fileName != "")
			{
				OpenMItem.IsEnabled = true;
				OpenFile.IsEnabled = true;
				SaveBtn.IsEnabled = true;
				
				var existingTrails = 0;

				_abcSessionList = _abcModelList.trials.ToList();

				_abcModelList.trials.All(abcSessObj =>
				{
					var tempAbcSessionObj = new ABCSessionModel();

					tempAbcSessionObj.Reference = GetFileNameFromPath(abcSessObj.Reference);
					tempAbcSessionObj.Sample = GetFileNameFromPath(abcSessObj.Sample);
					tempAbcSessionObj.TrialName = abcSessObj.TrialName;
					tempAbcSessionObj.TrialNumber = abcSessObj.TrialNumber;
					tempAbcSessionObj.HiddenReference = abcSessObj.HiddenReference;
					SessionGrid.Items.Add(tempAbcSessionObj);
					existingTrails++;
					return true;
				});

				_trialNumber = existingTrails;
				filesWithFullPath = _mediaPool.ToList();
                MediaPool.Items?.Clear();

                _mediaPool.All(items =>
				{
					MediaPool.Items.Add(GetFileNameFromPath(items));
					return true;
				});
				ScaleList.SelectedIndex = _abcModelList.Index;
			}
			else
			{
				OpenMItem.IsEnabled = false;
				OpenFile.IsEnabled = false;

				SessionGrid.Items.Clear();
				MediaPool.Items.Clear();
				_trialNumber = 0;
			}
			_prevtrialNumber = _trialNumber;
			//this.MouseEnter -= DataGridRow_Enter;
		}

		private void Addtrial_Click(object sender, RoutedEventArgs e)
		{
			_prevtrialNumber = _trialNumber;
            if (SessionGrid.Items.Count == 0)
            {
                _trialNumber = 0;
            }
            else
            {
                _trialNumber = SessionGrid.Items.Count;
            }
            _trialNumber += 1;
			AddSessionToList();
		}

        private void Deletetrial_Click(object sender, RoutedEventArgs e)
        {
            DeleteSessionFromList();
            SessionGrid.Items.Refresh();
        }
        private void OnSaveAsClicked(object sender, RoutedEventArgs e)
		{
			if (ValidateSessionBeforeSaving() == false)
			{
				System.Windows.Forms.MessageBox.Show("Can't save the session without Refernce and Sample files", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Log.Write(LogLevel.Error, "Can't save the session without Refernce and Sample files");
			}
			else if (SaveSession())
			{
				_sessionAlreadySaved = true;
               
            }
            

        }

        private void SaveFile()
		{
			if (_fileName == "")
			{
				var saveFileDialog = new SaveFileDialog();
				saveFileDialog.Filter = "Session Files |*.sess";
				if (saveFileDialog.ShowDialog() == DialogResult.OK)
				{
					
					_sessionLogger.FileName = saveFileDialog.FileName;
					_fileName = saveFileDialog.FileName;
					_sessionLogger.InitializeLogger(_abcModelList);
					_sessionFileFlag = true;
				}
				else
				{
					_sessionFileFlag = false;
				}
			}
			else
			{
				_abcModelList.trials.Clear();
				_sessionLogger.FileName = _fileName;
				_sessionLogger.InitializeLogger(_abcModelList);
				_sessionFileFlag = true;
			}
		}
		private void FlushSessionValues()
		{
			_abcSession.TrialNumber = 0;
			_abcSession.TrialName = string.Empty;
			_abcSession.Reference = string.Empty;
			_abcSession.Sample = string.Empty;
			_abcSession.HiddenReference = string.Empty;
		}
		private void AddSessionToList()
		{
			_abcSession = new ABCSessionModel();

			if(_abcSession == null)
			{
				throw new NullReferenceException("The ABC Session ViewModel is not initialized!!!!");
			}

			FlushSessionValues();
			_abcSessionList.Add(new ABCSessionModel { TrialNumber = _trialNumber });
			
			_abcSession.TrialNumber = _trialNumber;
			SessionGrid.Items.Add(_abcSession);
            
		}

        bool _deleteSelectedRow = false;
        private void DeleteSessionFromList()
        {
            var trialCount = SessionGrid.Items.Count;
            if (trialCount == 0)
            {
                return ;
            }
            _abcSession = SessionGrid.Items.GetItemAt(trialCount - 1) as ABCSessionModel;

            if (!_deleteSelectedRow)
            {
                //var trialCount = SessionGrid.Items.Count;
                if (trialCount > 0)
                {
                   // _abcSession = SessionGrid.Items.GetItemAt(trialCount - 1) as ABCSessionModel;
                    SessionGrid.Items.Remove(_abcSession);
					_abcSessionList.Remove(_abcSessionList.Where(item => item.TrialNumber == _abcSession.TrialNumber).FirstOrDefault());
				}
            }
            else
            {
                var list = (SessionGrid.SelectedItems.Cast<ABCSessionModel>().Select(sess => sess)).ToList();
                _abcSessionObjectToDelete.ToList().ForEach(abcSessObj => 
                {
                    SessionGrid.Items.Remove(abcSessObj);
					_abcSessionList.Remove(_abcSessionList.Where(item => item.TrialNumber == abcSessObj.TrialNumber).FirstOrDefault());
				});

                if(list.Count > 0)
                {
                    list.ForEach(sessObj => 
                    {
                        SessionGrid.Items.Remove(sessObj);
						_abcSessionList.Remove(_abcSessionList.Where(item => item.TrialNumber == sessObj.TrialNumber).FirstOrDefault());
					});
                }
                
            }
            ReEnumerateTrials();
            _deleteSelectedRow = false;
        }

        private void ReEnumerateTrials()
        {
            var sessCollection = SessionGrid.Items.SourceCollection;
            var sessionList = (sessCollection.Cast<ABCSessionModel>().Select(sess => sess)).ToList();

            var count = 0;
			sessionList.ToList().ForEach(sessObj => 
            {
                count++;
                sessObj.TrialNumber = count;
            });

			count = 0;
			_abcSessionList.ToList().ForEach(sessObj =>
			{
				count++;
				sessObj.TrialNumber = count;
			});
		}
		#region DataGrid and Media Pool Events
		private void Row_DblClick(object sender, MouseButtonEventArgs e)
		{
			DataGridRow row = sender as DataGridRow;
			DisplaytrialsWindow(row.GetIndex());
			ResetGridProperties();
		}

        IList<ABCSessionModel> _abcSessionObjectToDelete = new List<ABCSessionModel>();
		private void Row_Click(object sender, MouseButtonEventArgs e)
		{
			DataGridRow row = sender as DataGridRow;
            _abcSessionObjectToDelete.Clear();
            var rowNumber = row.GetIndex();
			//_abcSession = SessionGrid.Items.GetItemAt(rowNumber) as ABCSessionModel;
			//_abcSessionObjectToDelete.Add(_abcSession);

			_abcSessionObjectToDelete = (SessionGrid.SelectedItems.Cast<ABCSessionModel>().Select(sess => sess)).ToList(); 	        
            _deleteSelectedRow = true;

        }
		
		private void DataGridRow_Enter(object sender, System.Windows.Input.MouseEventArgs e)
		{
			var currentRow = sender as DataGridRow;
			if (currentRow == null)
			{
				return;
			}
			var currentRowIndex = currentRow.GetIndex();
			if (_dropContentFlag)
			{
				//var currentRow = sender as DataGridRow;
				if(currentRow == null)
				{
					return;
				}
				currentRowIndex = currentRow.GetIndex();
				_abcSession = SessionGrid.Items.GetItemAt(currentRowIndex) as ABCSessionModel;
				if (_abcSession.TrialName == "" )
				{
					
					_abcSession.TrialName = string.Format("Trial{0}", currentRowIndex + 1);
				}
				if(_abcSession.HiddenReference == "")
				{
					_abcSession.HiddenReference = "B";
				}
				FillDataFromGridToSession(currentRowIndex);
				_dropContentFlag = false;
			}
			else
			{
				//this.MouseEnter -= DataGridRow_Enter;
				e.Handled = true;
			}

			//OCT-05-2018 code 
			var value = "";
			if (_datagridColumnIndex == 1)
			{
				value = _abcSessionList[currentRowIndex].Reference;
			}
			if (_datagridColumnIndex == 2)
			{
				value = _abcSessionList[currentRowIndex].Sample;
			}
			//
			//var value = ((TextBlock)currentObject).Text;
			if (value != "")
			{
				ToolTipService.SetToolTip(_dataGridCurrentCell, new System.Windows.Controls.ToolTip { Content = value });
			}

		}
		private string GetFileNameFromPath(string fullpath)
		{
			var tokens = fullpath.Split('\\');
			return tokens[tokens.Length - 1];
		}

        string copyOfReference = "";
        string _copyOfTrialSamples = null;

        private void FillDataFromGridToSession(int currentRowIndex)
        {                     
            _abcSessionList[currentRowIndex].TrialName = _abcSession.TrialName;

            if (!string.IsNullOrEmpty(_abcSessionList?[currentRowIndex].Reference))
            {
                _copyOfTrialSamples = string.Copy(_abcSessionList[currentRowIndex].Sample);
                copyOfReference = string.Copy(_abcSessionList[currentRowIndex].Reference);
            }

            if (string.IsNullOrEmpty(_abcSessionList[currentRowIndex].HiddenReference))
            {
                _abcSessionList[currentRowIndex].HiddenReference = "B";
            }

            if (_datagridColumnIndex == 1 && _data != "")
            {
                _abcSession.Reference = GetFileNameFromPath(_data);
                _abcSessionList[currentRowIndex].Reference = _data;
            }
            else if (_datagridColumnIndex == 2 && _data != "")
            {
                _abcSession.Sample = GetFileNameFromPath(_data);
                _abcSessionList[currentRowIndex].Sample = _data;
            }
     

            if (_data != "")
            {
                ValidateTrails(currentRowIndex);
            }

            _data = "";
           // if (ValidateTrails(currentRowIndex)==true)
            if (_abcSessionList[currentRowIndex].Reference != "")
            {
                if (_abcSessionList[currentRowIndex].Sample == "Deleted")
                {
                    System.Windows.MessageBox.Show("The Sample(s) can't fit into the current trial because of different sampling rates as of Reference, The drag is revoked", "ERROR",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    // }

                    if (_abcSessionList[currentRowIndex].Sample == "Deleted")
                    {
                        _abcSessionList[currentRowIndex].Sample = "";
                        _abcSession.Sample = "";
                    }

                    if (!string.IsNullOrEmpty(_copyOfTrialSamples))
                    {

                        _abcSessionList[currentRowIndex].Reference = copyOfReference;
                        _abcSessionList[currentRowIndex].Sample = _copyOfTrialSamples;

                        _abcSession.Reference = GetFileNameFromPath(copyOfReference);
                        _abcSession.Sample = GetFileNameFromPath(_copyOfTrialSamples);
                        _copyOfTrialSamples = "";
                        copyOfReference = "";

                    }
                }
               
            }
            _sessionAlreadySaved = false;
			DataContext = _abcSession;
			SessionGrid.Items.Refresh();
		}

		private void ValidateTrails(int currentRowIndex)
		{
			var abcTrialObj = new ABCtrialWindow(_abcSession);
			if (_abcSessionList[currentRowIndex].Reference == "" || _abcSessionList[currentRowIndex].Reference == null)
			{
				System.Windows.Forms.MessageBox.Show("Please select Reference file for validating samples", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			else if(!string.IsNullOrEmpty(_abcSessionList[currentRowIndex].Sample))
			{
                var sample = new List<string> { _abcSessionList[currentRowIndex].Sample };
               // var sample = _abcSessionList[currentRowIndex].Sample.ToList();

                if (!abcTrialObj.ValidateTrails(_abcSessionList[currentRowIndex].Reference, sample))
				{					
                        //sample.Clear();
                        //_abcSession.Sample = "";
                        _abcSessionList[currentRowIndex].Sample = sample[0];
                   
				}
			}
		}

		private void MediaPool_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			_startPoint = e.GetPosition(null);
		}

		private void MediaPool_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
		{
			var mousePosition = e.GetPosition(null);
			var diff = _startPoint - mousePosition;

			if (e.LeftButton == MouseButtonState.Pressed &&
			   (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
				Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
			{

				// Get the dragged ListViewItem
				var mediaPool = sender as System.Windows.Controls.ListBox;
				var mediaPoolItem = FindAnchestor<ListBoxItem>((DependencyObject)e.OriginalSource);
				if (mediaPoolItem == null)
				{
					return;
				}
				
				var selectedItems = filesWithFullPath[MediaPool.SelectedIndex];
				// Initialize the drag & drop operation
				var dragData = new System.Windows.DataObject("myFormat", selectedItems);
				DragDrop.DoDragDrop(mediaPoolItem, dragData, System.Windows.DragDropEffects.Move);
			}
		}

		private static T FindAnchestor<T>(DependencyObject current)
			where T : DependencyObject
		{
			do
			{
				if (current is T)
				{
					return (T)current;
				}
				current = VisualTreeHelper.GetParent(current);
			}
			while (current != null);
			return null;
		}

		string _data = "";
		bool _dropContentFlag = false;
		private void SessionGrid_Drop(object sender, System.Windows.DragEventArgs e)
		{

			if (e.Data.GetDataPresent("myFormat"))
			{
				//this.MouseEnter += DataGridRow_Enter;
				_data = (string)e.Data.GetData("myFormat");
				_dropContentFlag = true;
			}
		}

		int _datagridColumnIndex = 0;
		System.Windows.Controls.DataGridCell _dataGridCurrentCell;
		private void MouseEnterOnDataGridCell(object sender, System.Windows.Input.MouseEventArgs e)
		{

		//	var currentCell = sender as System.Windows.Controls.DataGridCell;
			_dataGridCurrentCell = sender as System.Windows.Controls.DataGridCell;
			_datagridColumnIndex = _dataGridCurrentCell.Column.DisplayIndex;
			
			if (_dataGridCurrentCell.Content.ToString() != "" && _data == "")
			{
				// do nothing
			}
			else
			{
				if (_datagridColumnIndex == 1 || _datagridColumnIndex == 2)
				{
					_dataGridCurrentCell.Content = _data;
					SessionGrid.CurrentCell = new DataGridCellInfo(_dataGridCurrentCell);                    
                }
			}
          //  SessionGrid.Items.Refresh();
            //var currentObject = _dataGridCurrentCell?.Content;
            //if (currentObject.GetType() != typeof(TextBlock))
            //{
            //	return;
            //}

        }
		#endregion

		private void ResetGridProperties()
		{
			var bkColor = (Color)ColorConverter.ConvertFromString("#E4EEF4");
			var altBkColor = (Color) ColorConverter.ConvertFromString("#F0F8FC");
			SessionGrid.Background = new SolidColorBrush(bkColor);
			SessionGrid.AlternatingRowBackground = new SolidColorBrush(altBkColor);
		}
		private void DisplaytrialsWindow(int rowNumber)
		{	
			_abcSession = SessionGrid.Items.GetItemAt(rowNumber) as ABCSessionModel;
			_abcSession.TrialNumber = rowNumber + 1;
			Window window = new Window
			{
				Title = "ABC trials Window",
				Content = new ABCtrialWindow(_abcSessionList[rowNumber])
			};
			window.SizeToContent = SizeToContent.WidthAndHeight;
			window.ResizeMode = ResizeMode.NoResize;
			window.Background = Brushes.CadetBlue;
			window.WindowStartupLocation = WindowStartupLocation.CenterScreen;

			if (window.ShowDialog() == true)
			{
				_abcSession.Reference = GetFileNameFromPath(_abcSessionList[rowNumber].Reference);
				_abcSession.Sample = GetFileNameFromPath(_abcSessionList[rowNumber].Sample);
				_abcSession.HiddenReference = _abcSessionList[rowNumber].HiddenReference;
				_abcSession.TrialName = _abcSessionList[rowNumber].TrialName;

				DataContext = _abcSession;
			}
		}
		
		private void CancelBtn_Click(object sender, RoutedEventArgs e)
		{
			bool flag = false;

			if (_sessionCreatedFlag == false)
			{
				_abcModelList.SessionFileName = "";
			}

			if (_prevtrialNumber == _trialNumber)
			{
				flag = true;
			}
			else if(!_sessionAlreadySaved)
			{
				if(System.Windows.Forms.MessageBox.Show("Your Changes are not saved, Are you sure you want to exit?", "Info", 
					MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					flag = true;
				}
			}
			else
			{
				flag = true;
			}

			if (flag)
			{
				var parentWindow = (Window)this.Parent;
				parentWindow.DialogResult = false;
				parentWindow.Close();
			}
		}

		List<string> filesWithFullPath = null;
		private void BrowseBtn_Click(object sender, RoutedEventArgs e)
		{
			var fileDialogObject = new Microsoft.Win32.OpenFileDialog();
			filesWithFullPath = fileDialogObject.BrowseMultipleFilesToOpen("Audio Files|*.wav").ToList();
			var fileNameList = fileDialogObject.SafeFileNames.ToList();

			if (fileNameList?.Count > 0)
			{
				fileNameList.ToList().ForEach(file =>
				{
					if (!MediaPool.Items.Contains(file))
					{
						MediaPool.Items.Add(file);
					}

				});
			}
		}

		private bool ValidateSessionBeforeSaving()
		{
			var collection = SessionGrid.Items.SourceCollection;
            var list = collection.Cast<ABCSessionModel>().ToList();

            if (list.Count > 0)
            {
                return list.All(sessObj =>
                        {
                            if (sessObj.Reference == "" || sessObj.Sample == "")
                            {
                                return false;
                            }
                            else
                            {
                                return true;
                            }
                        });
            }
            else
            {
                return false;
            }
		}
		private void OkBtn_Click(object sender, RoutedEventArgs e)
		{
			var sessionValidFlag = false;
			if (!_sessionAlreadySaved)
			{
				sessionValidFlag = ValidateSessionBeforeSaving();
				if (sessionValidFlag == false)
				{
					System.Windows.Forms.MessageBox.Show("Can't save the session without Reference and Sample files", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
					Log.Write(LogLevel.Error, "Can't save the session without Reference and Sample files");
					return;
				}
                else 
				{
                    var dialogResult = System.Windows.Forms.MessageBox.Show("The Session is not saved!!! To save click on Yes", "Info", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (dialogResult == DialogResult.Yes)
                    {
                        SaveSession();
                        _sessionAlreadySaved = true;
                        LoadAbcView();
                        InvokeEvents(sender, _abcModelList);
                    }
				}
			}
			else
			{
				LoadAbcView();
				InvokeEvents(sender, _abcModelList);
			}
			if (_sessionAlreadySaved)
			{

				var parentWindow = (Window)this.Parent;
				parentWindow.DialogResult = true;
				parentWindow.Close();
			}
			
		}

		public static void InvokeEvents(object sender, ABCTestDetailsModel e)
		{
			DisplaySession?.Invoke(sender, e);
			SessionCreated?.Invoke(sender, e);
			_sessionCreatedFlag = true;
		}

		public static void InvokeSessionClosed(object sender, ABCTestDetailsModel e)
		{
			SessionClosed?.Invoke(sender, e);
			_sessionCreatedFlag = false;
		}

		private bool SaveSession()
		{
			bool flag = false;
			//         var mediaPoolList = MediaPool.Items.SourceCollection;
			//          _mediaPool = (mediaPoolList.Cast<string>().Select(poolItem => poolItem)).ToList();
			// var dialogResult = System.Windows.Forms.MessageBox.Show("The Session is not saved!!! To save click on Yes", "Info", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
			//         if (dialogResult == DialogResult.Yes)
			//{
				SaveFile();
				if (_sessionFileFlag)
				{
					PopulateABCSessionList();
                    _sessionLogger.WriteFile();
					if (filesWithFullPath != null)
					{
						_sessionLogger.WriteMediaPool(filesWithFullPath);
					}
                    _abcModelList.SessionFileName = _sessionLogger.FileName;

					foreach (var obj in _abcSessionList)
					{
						var msg = string.Format("Trial {0} with Trial Name: {1} is Added to the Session File {2} ", obj.TrialNumber, obj.TrialName, _sessionLogger.FileName);
						Log.Write(LogLevel.Info, msg);
					}
					Log.Write(LogLevel.Info, "The ABC Session is created successfully");
					_sessionLogger.CloseFile();
					
					flag = true;
				}
				else
				{
					Log.Write(LogLevel.Info, "The ABC Session is Cancelled");
					flag = false;
				}
			//}
			//else if (dialogResult == DialogResult.No)
			//{
			//	flag = false;
			//}

			return flag;
		}
		private void PopulateABCSessionList()
		{
		//	var sessCollection = SessionGrid.Items.SourceCollection;
		//	_abcSessionList = (sessCollection.Cast<ABCSessionModel>().Select(sess => sess)).ToList();

            foreach (var obj in _abcSessionList)
			{
                _abcModelList.trials.Add(obj);
			}
			_abcModelList.ScaleName = ScaleList.Text;
            _abcModelList.Index = (short)ScaleList.SelectedIndex;
            _abcModelList.TestName = "Triple Stimulus Test / Hidden Reference / ABC Test";
		}
		private void OnScrambleTrialsClicked(object sender, RoutedEventArgs e)
		{
			var sessCollection = SessionGrid.Items.SourceCollection;
			var list = sessCollection.Cast<ABCSessionModel>().Select(sess => sess).ToList();

			var randomObj = new Random();
			
			sessCollection = list.OrderBy(item => randomObj.Next(_trialNumber + 1));
           _abcSessionList.OrderBy(item => randomObj.Next(_trialNumber + 1)).ToList();
            SessionGrid.Items.Clear();

			foreach (var item in sessCollection)
			{
				SessionGrid.Items.Add(item);
			}

            ReEnumerateTrials();

        }

		private void OnScrambleSamplesClicked(object sender, RoutedEventArgs e)
		{
            var sessCollection = SessionGrid.Items.SourceCollection;
            var list = sessCollection.Cast<ABCSessionModel>().Select(sess => sess).ToList();

            var randomObj = new Random();
            var randomNumber = randomObj.Next(0, list.Count);

            for (int count = 0; count < randomNumber; count++)
            {

                if (list[count].HiddenReference == "B")
                {
                    list[count].HiddenReference = "C";
                    _abcSessionList[count].HiddenReference = "C";
                }
                else
                {
                    list[count].HiddenReference = "B";
                    _abcSessionList[count].HiddenReference = "B";
                }

            }

            sessCollection = list.ToList();

            SessionGrid.Items.Clear();

            foreach (var item in sessCollection)
            {
                SessionGrid.Items.Add(item);
            }

            ReEnumerateTrials();
        }

		private void OnOpenSessionClicked(object sender, RoutedEventArgs e)
		{
			var fileMenuObject = new FileMenuViewObject();
			var fileDialogObject = new Microsoft.Win32.OpenFileDialog();
			var fileName = fileDialogObject.BrowseFileToOpen("Session Files|*.sess");

			if (fileName != "")
			{
				if (fileMenuObject.ReadSessionFile(fileName))
				{
					_fileName = fileName;
					SessionGrid.Items.Clear();
					InitializeEditOrOpenSession();
				}
			}
		}
		
		private void ReadCustomScaleElements()
		{
			var fileName = "CustomScale.config";
			if (File.Exists(fileName))
			{
				var settings = new XmlReaderSettings();
				settings.ConformanceLevel = ConformanceLevel.Fragment;
				using (var reader = XmlReader.Create(fileName, settings))
				{
					while (reader.Read())
					{
						if (reader.NodeType == XmlNodeType.EndElement)
						{
							ScaleList.Items.Add(reader.Name);
						}
					}
				}
			}
		}
		public static void LoadAbcView()
		{
			Uri viewNav = new Uri("/ABCTestView", UriKind.Relative);
			UIModuleController._loadViewsInTransistion.RequestNavigate(Regions.WorkspaceRegion, viewNav);

			Uri viewNav1 = new Uri("/ScoringScales", UriKind.Relative);
			UIModuleController._loadViewsInTransistion.RequestNavigate(Regions.ScoreRegion, viewNav1);
		}

		
	}
}
