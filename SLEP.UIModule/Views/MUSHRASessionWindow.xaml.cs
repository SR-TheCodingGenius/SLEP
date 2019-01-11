using SLEP.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Forms;
using SLEP.SessionAndScore.Logger;
using SLEP.SessionAndScore.Logger.Handlers;
using SLEP.SessionAndScore.Logger.Enums;
using SLEP.UIModule.Controllers;
using Microsoft.Practices.Prism.Regions;
using SLEP.Logger;
using SLEP.UIModule.Menu;
using SLEP.Extensions.Handlers;
using System.IO;
using System.Xml;

namespace SLEP.UIModule.Views
{
    /// <summary>
    /// Interaction logic for MUSHRASessionWindow.xaml
    /// </summary>
    public partial class MUSHRASessionWindow : System.Windows.Controls.UserControl
    {
        private int _trialNumber = 0;
        private int _prevtrialNumber = 0;
        private MushraSessionModel _mushraSession;
        private IList<MushraSessionModel> _mushraSessionList = new List<MushraSessionModel>();
        private static MushraTestDetailsModel _mushraModelList;
        private bool _sessionFileFlag;
        private Point _startPoint;
        private string _fileName;
        private bool _sessionAlreadySaved = false;
        private string _selectedItem = "";
        private ISessionAndScoreLogger _sessionLogger;
        public static IList<string> _mediaPool;
        public static event EventHandler<MushraTestDetailsModel> DisplaySession;
        public static event EventHandler<MushraTestDetailsModel> SessionCreated;
        public static event EventHandler<MushraTestDetailsModel> SessionClosed;
        private static bool _sessionCreatedFlag = false;
        public MUSHRASessionWindow(string fileName = "")
        {
            InitializeComponent();

            _mushraSessionList = new List<MushraSessionModel>();
            _mushraModelList = MushraTestDetailsModel.GetInstance;

            _sessionLogger = LogFactory.GetLoggerObject((byte)LoggerType.SessionLogger);

            _fileName = fileName;
            _sessionAlreadySaved = false;
            InitializeEditOrOpenSession();
            ReadCustomScaleElements();
            _mushraModelList.trials.Clear();
        }

        private void InitializeEditOrOpenSession()
        {
            if (!string.IsNullOrEmpty(_fileName))
            {
                OpenMItem.IsEnabled = true;
                OpenFile.IsEnabled = true;
                var existingTrails = 0;

                _mushraSessionList = _mushraModelList.trials.ToList();

                _mushraModelList.trials.ToList().ForEach(mushraSessObj =>
                {
                    var tempMushraSessionObj = new MushraSessionModel();
                    tempMushraSessionObj.Reference = GetFileNameFromPath(mushraSessObj.Reference);
                    tempMushraSessionObj.TrialName = mushraSessObj.TrialName;
                    tempMushraSessionObj.TrialNumber = mushraSessObj.TrialNumber;

                    for (int count = 0; count < mushraSessObj.Sample.Count(); count++)
                    {
                        tempMushraSessionObj.Sample.Add(GetFileNameFromPath(mushraSessObj.Sample[count]));
                    }

                    MushraSessionGrid.Items.Add(tempMushraSessionObj);
                    existingTrails++;
                });

                _trialNumber = existingTrails;
                _mediapoolFilesWithFullPath = _mediaPool.ToList();
                MediaPool.Items?.Clear();
                _mediaPool.All(items =>
                {
                    MediaPool.Items.Add(GetFileNameFromPath(items));
                    return true;
                });

                ScaleList.SelectedIndex = _mushraModelList.Index;
            }
            else
            {
                OpenMItem.IsEnabled = false;
                OpenFile.IsEnabled = false;
                MushraSessionGrid.Items.Clear();
                MediaPool.Items.Clear();
                _trialNumber = 0;
            }

            _prevtrialNumber = _trialNumber;
        }


        private void AddNewTrial(object sender, RoutedEventArgs e)
        {
            _prevtrialNumber = _trialNumber;
            _trialNumber = MushraSessionGrid.Items.Count == 0 ? 0 : MushraSessionGrid.Items.Count;
            _trialNumber += 1;
            AddSessionToList();
        }

        private void DeleteTrial_Click(object sender, RoutedEventArgs e)
        {
            DeleteSessionFromList();
        }
        private void AddSessionToList()
        {
            _mushraSession = new MushraSessionModel();
            if (_mushraSession == null)
            {
                throw new NullReferenceException("The MUSHRA Session ViewModel is not initialized!!!!");
            }
            _mushraSession.TrialNumber = _trialNumber;
            MushraSessionGrid.Items.Add(_mushraSession);
            _mushraSessionList.Add(new MushraSessionModel { TrialNumber = _trialNumber });
        }

        bool _deleteSelectedRow;
        List<MushraSessionModel> _mushraSessionObjectToDelete = new List<MushraSessionModel>();
        private void DeleteSessionFromList()
        {

            if (!_deleteSelectedRow)
            {
                var trialCount = MushraSessionGrid.Items.Count;
                if (trialCount > 0)
                {
                    _mushraSession = MushraSessionGrid.Items.GetItemAt(trialCount - 1) as MushraSessionModel;
                    MushraSessionGrid.Items.Remove(_mushraSession);
                    _mushraSessionList.Remove(_mushraSessionList.Where(item => item.TrialNumber == _mushraSession.TrialNumber).FirstOrDefault());
                }
            }
            else
            {
                var list = (MushraSessionGrid.SelectedItems.Cast<MushraSessionModel>().Select(sess => sess)).ToList();
                _mushraSessionObjectToDelete.ForEach(mushraSessObj =>
                {
                    MushraSessionGrid.Items.Remove(mushraSessObj);
                    _mushraSessionList.Remove(_mushraSessionList.Where(item => item.TrialNumber == mushraSessObj.TrialNumber).FirstOrDefault());
                    //_mushraSessionList.Remove(mushraSessObj);
                });

                if (list.Count > 0)
                {
                    list.ForEach(sessObj =>
                    {
                        MushraSessionGrid.Items.Remove(sessObj);
                        _mushraSessionList.Remove(_mushraSessionList.Where(item => item.TrialNumber == sessObj.TrialNumber).FirstOrDefault());
                        //_mushraSessionList.Remove(sessObj);
                    });
                }

            }
            ReEnumerateTrials();
            _deleteSelectedRow = false;
        }

        private void ReEnumerateTrials()
        {
            var sessCollection = MushraSessionGrid.Items.SourceCollection;
            var sessionList = (sessCollection.Cast<MushraSessionModel>().Select(sess => sess)).ToList();
            var count = 0;

            sessionList.ToList().ForEach(sessObj =>
            {
                count++;
                sessObj.TrialNumber = count;
            });

            count = 0;
            _mushraSessionList.ToList().ForEach(sessObj =>
            {
                count++;
                sessObj.TrialNumber = count;
            });

        }

        #region DataGrid and Media Pool Events
        private void Row_DblClick(object sender, MouseButtonEventArgs e)
        {
            var row = sender as DataGridRow;
            if (row == null)
            {
                return;
            }
            DisplaytrialsWindow(row.GetIndex());
            ResetGridProperties();
        }
        /// <summary>
        /// A callback method, when Mouse Clicks a row of Data Grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Row_Click(object sender, MouseButtonEventArgs e)
        {
            var row = sender as DataGridRow;
            if (row == null)
            {
                return;
            }
            _mushraSessionObjectToDelete.Clear();
            var rowNumber = row.GetIndex();
            _mushraSessionObjectToDelete = (MushraSessionGrid.SelectedItems.Cast<MushraSessionModel>().Select(sess => sess)).ToList();
            _deleteSelectedRow = true;
        }

        /// <summary>
        /// A callback method, when Mouse enters on a row of Data Grid
        /// </summary>
        /// <param name="sender"> </param>
        /// <param name="e"></param>
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
                //_mushraSession = MushraSessionGrid.Items.GetItemAt(currentRowIndex) as MushraSessionModel;
                _mushraSession = MushraSessionGrid.Items.GetItemAt(currentRowIndex) as MushraSessionModel;

                if (_mushraSession?.TrialName == "")
                {
                    _mushraSession.TrialName = string.Format("Trial{0}", currentRowIndex + 1);

                }
                FillDataFromGridToSession(currentRowIndex);
                _dropContentFlag = false;
            }
            else
            {
                e.Handled = true;
            }

            //OCT-05-2018 code 
            var value = "";

            if (_datagridColumnIndex == 1)
            {
                value = _mushraSessionList[currentRowIndex].Reference;
            }
            else if (_datagridColumnIndex > 1 && _datagridColumnIndex <= 13 &&
                (_datagridColumnIndex - 2) < _mushraSessionList[currentRowIndex].Sample.Count() &&
                _mushraSessionList[currentRowIndex].Sample.Count > 0)
            {
                value = _mushraSessionList[currentRowIndex].Sample[_datagridColumnIndex - 2];
            }
            //
            if (value != "")
            {
                ToolTipService.SetToolTip(_dataGridCurrentCell, new System.Windows.Controls.ToolTip { Content = value });
            }
        }


        private List<string> _copyOfTrialSamples = null;
        string copyOfReference = "";

        /// <summary>
        /// Takes the Mushra Data Grid as input and fills the data into 
        /// the underlying data structure of Mushra Session Object.
        /// </summary>
        /// 
        private void FillDataFromGridToSession(int currentIndex)
        {
            if (_mushraSessionList == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(_mushraSessionList?[currentIndex].Reference))
            {
                _copyOfTrialSamples = _mushraSessionList[currentIndex].Sample.ToList();
                copyOfReference = _mushraSessionList[currentIndex].Reference;
            }

            _mushraSessionList[currentIndex].TrialName = _mushraSession.TrialName;

            if (_datagridColumnIndex > 1 && _datagridColumnIndex <= 13 && _data != "")
            {
                if (_mushraSessionList[currentIndex].Sample.Count < _datagridColumnIndex - 1)
                {
                    _mushraSessionList[currentIndex].Sample.Add(_data);
                    _mushraSession.Sample.Add(GetFileNameFromPath(_data));
                }
                else if (_datagridColumnIndex == 2)
                {
                    if (_mushraSessionList[currentIndex].Sample[0] != _mushraSessionList[0].Reference)
                    {
                        _mushraSessionList[currentIndex].Sample[0] = _data;
                    }
                }
                else
                {
                    _mushraSessionList[currentIndex].Sample[_datagridColumnIndex - 2] = _data;
                    _mushraSession.Sample[_datagridColumnIndex - 2] = GetFileNameFromPath(_data);
                }

            }
            else if (_datagridColumnIndex == 1 && _data != "")
            {
                _mushraSessionList[currentIndex].Reference = _data;
                _mushraSession.Reference = GetFileNameFromPath(_data);

                if (_mushraSessionList[currentIndex].Sample.Count == 0)
                {
                    _mushraSessionList[currentIndex].Sample.Add(_data);
                    _mushraSession.Sample.Add(GetFileNameFromPath(_data));
                }
                else
                {
                    _mushraSessionList[currentIndex].Sample[0] = _data;
                    _mushraSession.Sample[0] = GetFileNameFromPath(_data);
                }

            }


            if (_data != "")
            {
                ValidateTrails(currentIndex);
            }

            _data = "";



            if (_mushraSessionList[currentIndex].Reference != "")
            {
                var count = 0;
                var deleteCount = 0;

                if (_mushraSessionList[currentIndex].Sample.Any(item => (item == "Deleted") ? true : false))
                {
                    System.Windows.MessageBox.Show("The Sample(s) can't fit into the current trial because of different sampling rates as of Reference, The drag is revoked", "ERROR",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                if (_copyOfTrialSamples != null)
                {
                    for (; count < _copyOfTrialSamples.Count; count++)
                    {
                        if (_mushraSessionList?[currentIndex]?.Sample?[count] == "Deleted")
                        {
                            _mushraSessionList[currentIndex].Sample[count] = _copyOfTrialSamples[count];
                            _mushraSession.Sample[count] = GetFileNameFromPath(_copyOfTrialSamples[count]);
                            deleteCount++;
                        }
                    }

                    if (_mushraSessionList?[currentIndex]?.Sample?[_mushraSessionList[currentIndex].Sample.Count() - 1] == "Deleted")
                    {
                        _mushraSessionList[currentIndex].Sample.RemoveAt(_mushraSessionList[currentIndex].Sample.Count() - 1);
                        _mushraSession.Sample.RemoveAt(_mushraSession.Sample.Count() - 1);
                    }
                    if (deleteCount > 0 && deleteCount == count - 1)
                    {
                        _mushraSessionList[currentIndex].Reference = copyOfReference;
                        _mushraSessionList[currentIndex].Sample[0] = copyOfReference;

                        _mushraSession.Reference = GetFileNameFromPath(copyOfReference);
                        _mushraSession.Sample[0] = GetFileNameFromPath(copyOfReference);
                        deleteCount = 0;
                    }
                }
            }
            DataContext = _mushraSession;
            MushraSessionGrid.Items.Refresh();
        }

        private string GetFileNameFromPath(string fullpath)
        {
            var tokens = fullpath.Split('\\');
            return tokens[tokens.Length - 1];
        }
        private void ValidateTrails(int currentIndex)
        {
           
            var mushraTrialObj = new MushraTrialWindow(_mushraSessionList[currentIndex]);
            if (string.IsNullOrEmpty(_mushraSessionList[currentIndex].Reference))
            {
                _mushraSessionList[currentIndex].Reference = "";
                System.Windows.Forms.MessageBox.Show("Please Select Reference file for validating samples", "ERROR",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                _mushraSessionList[currentIndex].Sample.Clear();

                _mushraSession.Reference = "";
                _mushraSession.Sample.Clear();
            }
            else
            {
                mushraTrialObj.ValidateTrails(_mushraSessionList[currentIndex].Reference, _mushraSessionList[currentIndex].Sample);
                //mushraTrialObj.ValidateTrails(_mushraSession.Reference, _mushraSession.Sample);
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

                var mediaPoolItem = FindAnchestor<ListBoxItem>((DependencyObject)e.OriginalSource);
                if (mediaPoolItem == null)
                {
                    return;
                }

                _selectedItem = _mediapoolFilesWithFullPath[MediaPool.SelectedIndex];

                // Initialize the drag & drop operation
                var dragData = new System.Windows.DataObject("myFormat", _selectedItem);
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
        private string _data = "";
        private bool _dropContentFlag = false;
        private void MushraSessionGrid_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent("myFormat"))
            {
                //MouseEnter += DataGridRow_Enter;
                _data = (string)e.Data.GetData("myFormat");
                _dropContentFlag = true;
            }
        }

        private int _datagridColumnIndex;
        System.Windows.Controls.DataGridCell _dataGridCurrentCell;
        private void MouseEnterOnDataGridCell(object sender, System.Windows.Input.MouseEventArgs e)
        {

            _dataGridCurrentCell = sender as System.Windows.Controls.DataGridCell;
            if (_dataGridCurrentCell == null)
            {
                return;
            }

            _datagridColumnIndex = _dataGridCurrentCell.Column.DisplayIndex;

            if (!(_dataGridCurrentCell.Content.ToString() != "" && _data == ""))
            {
                if (_datagridColumnIndex >= 1 && _datagridColumnIndex <= 13)
                {
                    var fileNameTokens = _data.Split('\\');

                    //currentCell.Content = fileNameTokens?[fileNameTokens.Length - 1];//_data;
                    _dataGridCurrentCell.Content = _data;
                    MushraSessionGrid.CurrentCell = new DataGridCellInfo(_dataGridCurrentCell);
                }
            }

            //var currentObject = _dataGridCurrentCell?.Content;
            //if (currentObject.GetType() != typeof(TextBlock))
            //{
            //	return;
            //}

            //var value = ((TextBlock)currentObject).Text;
            //if (value != "")
            //{
            //	ToolTipService.SetToolTip(_dataGridCurrentCell, new System.Windows.Controls.ToolTip { Content = value });
            //}

        }

        #endregion
        private void ResetGridProperties()
        {
            var bkColor = (Color)ColorConverter.ConvertFromString("#E4EEF4");
            var altBkColor = (Color)ColorConverter.ConvertFromString("#F0F8FC");
            MushraSessionGrid.Background = new SolidColorBrush(bkColor);
            MushraSessionGrid.AlternatingRowBackground = new SolidColorBrush(altBkColor);
        }

        private void DisplaytrialsWindow(int rowNumber)
        {
            //_mushraSession = MushraSessionGrid.Items.GetItemAt(rowNumber) as MushraSessionModel;
            //if (_mushraSession == null)
            //{
            //	return;
            //}
            _mushraSessionList[rowNumber].TrialNumber = rowNumber + 1;
            Window window = new Window
            {
                Title = "MUSHRA Trials Window",
                Content = new MushraTrialWindow(_mushraSessionList[rowNumber])
            };
            window.SizeToContent = SizeToContent.WidthAndHeight;
            window.ResizeMode = ResizeMode.NoResize;
            window.Background = Brushes.CadetBlue;
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            if (window.ShowDialog() == true)
            {
             
                DataContext = _mushraSessionList[rowNumber]; 
               // MushraSessionGrid.Items.SourceCollection = _mushraSessionList;
                MushraSessionGrid.Items.Refresh();
            }
        }

        private void PopulateMushraSessionList()
        {
            _mushraModelList.trials?.Clear();
            foreach (var obj in _mushraSessionList)
            {
                if (obj.HiddenReference == null)
                {
                    obj.HiddenReference = "A";
                }
                _mushraModelList.trials.Add(obj);
            }
            _mushraModelList.ScaleName = ScaleList.Text;
            _mushraModelList.Index = (short)ScaleList.SelectedIndex;
            _mushraModelList.TestName = "MUSHRA Test";
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
                    _sessionLogger.InitializeLogger(_mushraModelList);
                    _sessionFileFlag = true;
                }
                else
                {
                    _sessionFileFlag = false;
                }
            }
            else
            {
                _sessionLogger.FileName = _fileName;
                _sessionLogger.InitializeLogger(_mushraModelList);
                _sessionFileFlag = true;
            }
        }

        private bool SaveSession()
        {
            var flag = false;
            //var mediaPoolList = MediaPool.Items.SourceCollection;
            //_mediaPool = (mediaPoolList.Cast<string>().Select(poolItem => poolItem)).ToList();
            //var dialogResult = System.Windows.Forms.MessageBox.Show("The Session is not saved!!! To save click on Yes", "Info", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            //if (dialogResult == DialogResult.Yes)
           // {
                SaveFile();
                if (_sessionFileFlag)
                {
                    PopulateMushraSessionList();
                    _sessionLogger.WriteFile();

					if (_mediapoolFilesWithFullPath != null)
					{
						_sessionLogger.WriteMediaPool(_mediapoolFilesWithFullPath);//_mediapoolFilesWithFullPath
					}
				   _mushraModelList.SessionFileName = _sessionLogger.FileName;
                    foreach (var obj in _mushraSessionList)
                    {
                        var msg = string.Format("Trial {0} with Trial Name: {1} is Added to the Session File {2} ", obj.TrialNumber, obj.TrialName, _sessionLogger.FileName);
                        Log.Write(LogLevel.Info, msg);
                    }
                    Log.Write(LogLevel.Info, "The MUSHRA Session is created successfully");
                    _sessionLogger.CloseFile();

                    flag = true;
                }
                else
                {
                    Log.Write(LogLevel.Info, "The MUSHRA Session is Cancelled");
                    flag = false;
                }
           // }
            //else if (dialogResult == DialogResult.No)
            //{

              //  flag = false;
            //}

            return flag;
        }

        private bool ValidateSessionBeforeSaving()
        {
            if (_mushraSessionList.Count > 0)
            {
                var result = _mushraSessionList.All(sessObj =>
                                    {
                                        if (sessObj.Reference == "" || sessObj.Sample.All(sample => sample == "") == true)
                                        {
                                            return false;
                                        }
                                        else
                                        {
                                            return true;
                                        }
                                    });

                _mushraSessionList.ToList().ForEach(session =>
                {
                    var samplCount = 0;
                    session.Sample.ToList().ForEach(sampl =>
                    {
                        if (sampl != "")
                        {
                            samplCount++;
                        }
                    });
                    if (samplCount == 0)
                    {
                        result = false;
                    }
                    else if (samplCount < 2)
                    {
                        System.Windows.Forms.MessageBox.Show("Mushra Test needs minimum of 1 sample other than Hidden Reference", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        result = false;
                    }
                });

                return result;
            }
            else
            {
                return false;
            }
        }
        private void OnOKBtn_Clicked(object sender, RoutedEventArgs e)
        {
            var sessionValidFlag = false;
            if (!_sessionAlreadySaved)
            {
                sessionValidFlag = ValidateSessionBeforeSaving();
                if (sessionValidFlag == false)
                {
                    System.Windows.Forms.MessageBox.Show("Can't save the session without Refernce and Sample files", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Log.Write(LogLevel.Error, "Can't save the session without Refernce and Sample files");
                }
                else if (SaveSession())
                {
                    _sessionAlreadySaved = true;
                    LoadMushraView();
                    InvokeEvents(sender, _mushraModelList);
                }
            }
            else
            {
                LoadMushraView();
                InvokeEvents(sender, _mushraModelList);
            }

            if (_sessionAlreadySaved)
            {
                var parentWindow = (Window)Parent;
                parentWindow.DialogResult = true;
                parentWindow.Close();
            }
        }

        private void OnCancelBtn_Clicked(object sender, RoutedEventArgs e)
        {
            if (_sessionCreatedFlag == false)
            {
                _mushraModelList.SessionFileName = "";
            }
            if (_prevtrialNumber == _trialNumber)
            {
                var parentWindow = (Window)this.Parent;
                parentWindow.DialogResult = false;
                parentWindow.Close();
            }
            else if (!_sessionAlreadySaved)
            {
                if (System.Windows.Forms.MessageBox.Show("Your Changes are not saved, Are you sure you want to exit?", "Info",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    var parentWindow = (Window)this.Parent;
                    parentWindow.DialogResult = false;
                    parentWindow.Close();
                }
            }
        }

        public static void LoadMushraView()
        {
            Uri viewNav = new Uri("/MushraTestTransportControlsView", UriKind.Relative);
            UIModuleController._loadViewsInTransistion.RequestNavigate(Regions.WorkspaceRegion, viewNav);

            Uri viewNav1 = new Uri("/MUSHRATestScoreView", UriKind.Relative);
            UIModuleController._loadViewsInTransistion.RequestNavigate(Regions.ScoreRegion, viewNav1);
        }
        public static void InvokeEvents(object sender, MushraTestDetailsModel e)
        {

            DisplaySession?.Invoke(sender, e);
            SessionCreated?.Invoke(sender, e);
            _sessionCreatedFlag = true;
        }

        public static void InvokeSessionClosed(object sender, MushraTestDetailsModel e)
        {
            SessionClosed?.Invoke(sender, e);
            _sessionCreatedFlag = false;
        }

        List<string> _mediapoolFilesWithFullPath = null;
        private void BrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            var fileDialogObject = new Microsoft.Win32.OpenFileDialog();
            _mediapoolFilesWithFullPath = fileDialogObject.BrowseMultipleFilesToOpen("Audio Files|*.wav").ToList();
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
                    MushraSessionGrid.Items.Clear();
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

        private void OnScrambleTrailsClicked(object sender, RoutedEventArgs e)
        {
            var sessCollection = MushraSessionGrid.Items.SourceCollection;
            var trialList = sessCollection.Cast<MushraSessionModel>().Select(sess => sess).ToList();

            var randomObj = new Random();

            var listA = trialList.ToList();
            var listB = _mushraSessionList.ToList();
            var zippedList = listA.Zip(listB, (a, b) => new { a, b }).ToList();

            trialList = zippedList.Select(pair => pair.a).ToList();
            sessCollection = trialList.OrderBy(item => randomObj.Next(_trialNumber));

            _mushraSessionList = zippedList.Select(pair => pair.b).ToList();

            MushraSessionGrid.Items.Clear();
            foreach (var item in sessCollection)
            {
                MushraSessionGrid.Items.Add(item);
            }
            ReEnumerateTrials();
        }

        private void OnScrambleSamplesClicked(object sender, RoutedEventArgs e)
        {
            var sessCollection = MushraSessionGrid.Items.SourceCollection;
            var trialList = sessCollection.Cast<MushraSessionModel>().Select(sess => sess).ToList();

            var randomObj = new Random();
            for (int count = 0; count < trialList.Count; count++)
            {
                var listA = trialList[count].Sample.ToList();
                var listB = _mushraSessionList[count].Sample.ToList();
                var zippedList = listA.Zip(listB, (a, b) => new { a, b }).ToList();

                zippedList = zippedList.OrderBy(item => randomObj.Next()).ToList();

                trialList[count].Sample = zippedList.Select(pair => pair.a).ToList();
                _mushraSessionList[count].Sample = zippedList.Select(pair => pair.b).ToList();
            }

            sessCollection = trialList;
            ReEnumerateTrials();
        }

    }
}
