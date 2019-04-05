using SLEP.Extensions.Handlers;
using SLEP.Models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.IO;
using Microsoft.Win32;
using System;

namespace SLEP.UIModule.Views
{
    /// <summary>
    /// Interaction logic for ScoreMerge.xaml
    /// </summary>
    public partial class ScoreMerge : UserControl
    {
        private ViewScores _viewScoresObj;
        private ScoreFileDetails _currentScoreFileDetailsObj = null;
        private ScoreFileDetails _previousScoreFileDetailsObj = null;
        private IList<ScoreFileDetails> _scoreFileDetailsList = new List<ScoreFileDetails>();
        private IDictionary<int, List<string>> _fileScoresByUsers = new Dictionary<int, List<string>>();
        private FileStream _fileStream = null;

        public ScoreMerge()
        {
            InitializeComponent();
            _viewScoresObj = new ViewScores();
        }

        private void OnAddClicked(object sender, RoutedEventArgs e)
        {
            var fileDialogObject = new Microsoft.Win32.OpenFileDialog();
            var fileName = fileDialogObject.BrowseFileToOpen("Score Files|*.sco");
            if (fileName != "")
            {
                if (FileListBox.Items.Count == 0)
                {
                    _previousScoreFileDetailsObj = null;
                }

                _currentScoreFileDetailsObj = new ScoreFileDetails();
                ViewScores.FileName = fileName;

                if (ValidateScoreFiles())
                {
                    _previousScoreFileDetailsObj = _currentScoreFileDetailsObj;
                    _scoreFileDetailsList.Add(_currentScoreFileDetailsObj);
                    FileListBox.Items.Add(fileName);
                }
                else
                {
                    MessageBox.Show("This file can't be merged!!!! as there is a mismatch in one of the following \t\n 1.Session Name \t\n 2.Scale Name \t\n 3.Trials", "Info",
                        MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }

        }

        private bool ValidateScoreFiles()
        {
            bool? result = false;

            if (_viewScoresObj.ReadScoreFile(_currentScoreFileDetailsObj))
            {
                if (_previousScoreFileDetailsObj == null)
                {
                    result = true;
                }
                else
                {
                    _previousScoreFileDetailsObj.SessionName = _previousScoreFileDetailsObj.SessionName.Trim();
                    _previousScoreFileDetailsObj.ScaleName = _previousScoreFileDetailsObj.ScaleName.Trim();
                    _currentScoreFileDetailsObj.SessionName = _currentScoreFileDetailsObj.SessionName.Trim();
                    _currentScoreFileDetailsObj.ScaleName = _currentScoreFileDetailsObj.ScaleName.Trim();
                }


                if (_previousScoreFileDetailsObj?.SessionName == _currentScoreFileDetailsObj?.SessionName && _previousScoreFileDetailsObj?.ScaleName == _currentScoreFileDetailsObj?.ScaleName)
                {
                    result = _previousScoreFileDetailsObj.ScoresList.All(prevItem =>
                            {
                                return _currentScoreFileDetailsObj.ScoresList.Any(currItem =>
                                {
                                    if (currItem.File == prevItem.File && currItem.Trial == prevItem.Trial)
                                    {
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }

                                });

                            });

                }

            }
            else
            {
                return false;
            }

            return (bool)result;
        }
        private void OnRemoveClicked(object sender, RoutedEventArgs e)
        {
            if (FileListBox.Items.Count == 0)
            {
                MessageBox.Show("No Files listed in the list box", "Info", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            else if (FileListBox.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a file from listbox to remove", "Info", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            _scoreFileDetailsList.RemoveAt(FileListBox.SelectedIndex);
            FileListBox.Items.RemoveAt(FileListBox.SelectedIndex);
        }

        private void OnRemoveAllClicked(object sender, RoutedEventArgs e)
        {
            if (CheckFilesInListBox(1) == false)
            {
                return;
            }
            FileListBox.Items.Clear();
            _scoreFileDetailsList.Clear();
            _fileScoresByUsers.Clear();
        }

        private void OnViewClicked(object sender, RoutedEventArgs e)
        {
            if (CheckFilesInListBox(2) == false)
            {
                return;
            }
            else if (FileListBox.SelectedItem == null)
            {
                MessageBox.Show("Please select file to view the score", "Info", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            ViewScores.FileName = FileListBox.SelectedItem.ToString();
            Window window = new Window
            {
                Content = _viewScoresObj
            };

            window.SetWindowProperties();
            window.ShowDialog();

        }

        private bool CheckFilesInListBox(int item)
        {
            bool flag = true;
            if (item == 1 || item == 2 || item == 3)
            {
                if (FileListBox.Items.Count == 0)
                {
                    MessageBox.Show("No Files listed in the list box", "Info", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    flag = false;
                }
            }
            if (item == 2)
            {
                if (FileListBox.Items.Count > 0)
                {
                    if (FileListBox.SelectedItem == null)
                    {
                        MessageBox.Show("Please select file to view the score", "Info", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        flag = false;
                    }
                }
            }

            if (item == 3)
            {
                if (FileListBox.Items.Count > 0)
                {
                    if (OutputFileText.Text == string.Empty)
                    {
                        MessageBox.Show("Output File is mandatory", "Info", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return false;
                    }
                }
            }

            return flag;

        }
        private void OnMergeClicked(object sender, RoutedEventArgs e)
        {

            if (CheckFilesInListBox(3) == false)
            {
                return;
            }


            var firstScoreFileObj = _scoreFileDetailsList.FirstOrDefault().ScoresList;

            for (var item = 0; item < firstScoreFileObj.Count(); item++)
            {
                _fileScoresByUsers.Add((item + 1), new List<string>());
            }

            var Key = 1;
            _scoreFileDetailsList.ToList().ForEach(scoreFile =>
            {
                var scoreList = scoreFile.ScoresList.ToList();
              
                for (int count = 0; count < firstScoreFileObj.Count;)
                {
                    for (int count2 = 0; count2 < firstScoreFileObj.Count;)
                    {
                        if (firstScoreFileObj[count].File == scoreList[count2].File && firstScoreFileObj[count].Trial == scoreList[count2].Trial)
                        {
                            _fileScoresByUsers[Key].Add(scoreList[count2].Score);
                            Key += 1;
                            count++;
                            count2++;
                        }
                    }

                }
                Key = 1;

            });
            WriteMergedScoresToDisk();

            _fileScoresByUsers.Clear();
            _scoreFileDetailsList.Clear();
            FileListBox.Items.Clear();
            OutputFileText.Clear();
            MessageBox.Show("Scores are Merged Successfully", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void WriteMergedScoresToDisk()
        {
            try
            {

                using (var streamWriter = new StreamWriter(_fileStream))
                {
                    var scoreFileList = _scoreFileDetailsList.ToList();
                    var firstScoreFile = scoreFileList.FirstOrDefault();
                    streamWriter.Write("File\t");
                    scoreFileList.ForEach(scoreFile =>
                    {

                        streamWriter.Write(string.Format("{0}\t", scoreFile.TesterName));
                    });

                    streamWriter.Write(streamWriter.NewLine);

                    var Key = 1;
                    firstScoreFile.ScoresList.ForEach(item =>
                    {
                        streamWriter.Write(string.Format("{0}\t", item.File));
                        _fileScoresByUsers[Key].ForEach(score =>
                        {
                            streamWriter.Write(string.Format("{0}\t", score));
                        });
                        Key += 1;
                        streamWriter.Write(streamWriter.NewLine);
                    });


                    streamWriter.Close();
                }
            }
            catch(Exception)
            {

            }
        }

        private void OnQuitClicked(object sender, RoutedEventArgs e)
        {
            var parentWindow = (Window)this.Parent;
            parentWindow.DialogResult = false;
            parentWindow.Close();
        }

        private void BrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
			saveFileDialog.Filter = "Text Files|*.txt";

			var result = saveFileDialog.ShowDialog();
			if (result == true)
			{
				if (!File.Exists(saveFileDialog.FileName))
				{
					_fileStream = File.Create(saveFileDialog.FileName);								
				}
				OutputFileText.Text = saveFileDialog.FileName;
			}
		}
    }
}
