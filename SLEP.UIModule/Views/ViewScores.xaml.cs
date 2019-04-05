using SLEP.Extensions.Handlers;
using SLEP.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Forms;

namespace SLEP.UIModule.Views
{
	/// <summary>
	/// Interaction logic for ViewScores.xaml
	/// </summary>
	public partial class ViewScores : System.Windows.Controls.UserControl
	{
	
		
		public ViewScores()
		{
			InitializeComponent();
		
		}

		public static string FileName { get; set; }
		private void ViewScores_Loaded(object sender, RoutedEventArgs e)
		{
			SetWindowTitle();
			
			if (FileName != "")
			{
				var scoreFileHeaders = new ScoreFileDetails();
				

				if (ReadScoreFile(scoreFileHeaders))
				{
					sessionBlock.Text = scoreFileHeaders.SessionName;
					testNameBlock.Text = scoreFileHeaders.TestName;
					scaleNameBlock.Text = scoreFileHeaders.ScaleName;
					nameBlock.Text = scoreFileHeaders.TesterName;
					sexBlock.Text = scoreFileHeaders.Sex;
					ageBlock.Text = scoreFileHeaders.Age;
					notesTextbox.Text = scoreFileHeaders.TesterNotes;

					scoreGrid.Items.Clear();
					scoreFileHeaders.ScoresList.ForEach(score =>
					{
						scoreGrid.Items.Add(score);
					});
				}
			}
			else
			{
				System.Windows.Forms.MessageBox.Show("Score file is not yet created for the current session!!!!!!\n But You can browse other score files using File Menu", "Info",
					MessageBoxButtons.OK,MessageBoxIcon.Information);
			}
		}

		public bool ReadScoreFile(ScoreFileDetails scoreFileHeaders)
		{

			bool flag = true;
			string line = "";
			string[] index = null;
			bool afterNotesFlag = false;

			try
			{
                if (string.IsNullOrEmpty(FileName))
                {
                    System.Windows.Forms.MessageBox.Show("File does not exists\n\n", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
                   
				using (var streamReader = new StreamReader(FileName))
				{
					scoreFileHeaders.ScoresList = new List<ScoreDetails>();
					while ((line = streamReader.ReadLine()) != null)
					{
						if ((line.ToLower()).Contains("session file"))
						{
							index = line.Split('=');
							scoreFileHeaders.SessionName = index[1];							
						}

						if ((line.ToLower()).Contains("test method"))
						{
							index = line.Split('=');
							scoreFileHeaders.TestName = index[1];
						}
						if ((line.ToLower()).Contains("scale"))
						{
							index = line.Split('=');
							scoreFileHeaders.ScaleName = index[1];
						}

						if ((line.ToLower()).Contains("name"))
						{
							index = line.Split('=');
							scoreFileHeaders.TesterName = index[1];
						}
						if ((line.ToLower()).Contains("sex"))
						{
							index = line.Split('=');
							scoreFileHeaders.Sex = index[1];
						}
						if ((line.ToLower()).Contains("age"))
						{
							index = line.Split('=');
							scoreFileHeaders.Age = index[1];
						}
						if ((line.ToLower()).Contains("notes"))
						{
							index = line.Split('=');
							scoreFileHeaders.TesterNotes = index[1];
							afterNotesFlag = true;
						}

						if(afterNotesFlag)
						{
							if(line != "")
							{
								var subStrings = line.Split('\t');

								if(subStrings.Length > 3)
								{
									scoreFileHeaders.ScoresList.Add(new ScoreDetails
																	{
																		Trial = subStrings[0],
																		Button = subStrings[1],
																		File = subStrings[2],
																		Score = subStrings[3]
																	});
									
								}
							}
						}
						
					}
				}
			}
			catch (Exception )
			{
				System.Windows.Forms.MessageBox.Show(string.Format("The Reading of the Score File is Failed, The score file is corrupted\n\n" ), "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
				throw new Exception("The Reading of the Score File is Failed, The file is corrupted");
			}

			return flag;
		}

		private void SetWindowTitle()
		{
				var parentWindow = (Window)this.Parent;
				parentWindow.Title = string.Format("View Scores: {0}", FileName);
			
		}

		private void OnFileOpenClicked(object sender, RoutedEventArgs e)
		{
			//var openFileDialog = new OpenFileDialog();
			//openFileDialog.Filter = "Score Files|*.sco";
			var openFileDialog = new Microsoft.Win32.OpenFileDialog();
			var fileName = openFileDialog.BrowseFileToOpen("Score Files|*.sco");
			if (!string.IsNullOrEmpty(fileName))
			{
				FileName = fileName;
				SetWindowTitle();
			}
			else
			{
				FileName = "";
			}

			if (FileName != "")
			{
				var scoreFileHeaders = new ScoreFileDetails();
				
				if (ReadScoreFile(scoreFileHeaders))
				{
					sessionBlock.Text = scoreFileHeaders.SessionName;
					testNameBlock.Text = scoreFileHeaders.TestName;
					scaleNameBlock.Text = scoreFileHeaders.ScaleName;
					nameBlock.Text = scoreFileHeaders.TesterName;
					sexBlock.Text = scoreFileHeaders.Sex;
					ageBlock.Text = scoreFileHeaders.Age;
					notesTextbox.Text = scoreFileHeaders.TesterNotes;
					scoreGrid.Items.Clear();
					scoreFileHeaders.ScoresList.ForEach(score =>
					{
						scoreGrid.Items.Add(score);
					});
				}
			}
		}

		private void OnOkClicked(object sender, RoutedEventArgs e)
		{
			var parentWindow = (Window)this.Parent;
			parentWindow.Close();
		}
	}
}
