using SLEP.Audio;
using SLEP.Models;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using System.Linq;
using System.Windows.Documents;
using SLEP.Extensions.Handlers;

namespace SLEP.UIModule.Views
{
	/// <summary>
	/// Interaction logic for MushraTrialWindow.xaml
	/// </summary>
	public partial class MushraTrialWindow : System.Windows.Controls.UserControl
	{
		private MushraSessionModel _mushraSession;
        private List<System.Windows.Controls.TextBox> _textBoxList = new List<System.Windows.Controls.TextBox>();
		public MushraTrialWindow(MushraSessionModel mushraSession)
		{
			
			InitializeComponent();
			_mushraSession = mushraSession;

            _textBoxList.Add(FileA);
            _textBoxList.Add(FileB);
            _textBoxList.Add(FileC);
            _textBoxList.Add(FileD);
            _textBoxList.Add(FileE);
            _textBoxList.Add(FileF);
            _textBoxList.Add(FileG);
            _textBoxList.Add(FileH);
            _textBoxList.Add(FileI);
            _textBoxList.Add(FileJ);
            _textBoxList.Add(FileK);
            _textBoxList.Add(FileL);
            trialNumberText.Text = mushraSession.TrialNumber.ToString();
            LoadCurrentTrialDetails();
			EnableDisableOfBtnA();

		}

		private void EnableDisableOfBtnA()
		{
			if(FileRef.Text == FileA.Text)
			{
				BrowseBtnA.IsEnabled = false;
			}
			else
			{
				BrowseBtnA.IsEnabled = true;
			}
		}
		private void LoadCurrentTrialDetails()
		{
			trialNumberText.Text = _mushraSession.TrialNumber.ToString();
			trialNameText.Text = _mushraSession.TrialName;
			FileRef.Text = _mushraSession.Reference;
            var index = 0;
            
            foreach (var item in _mushraSession.Sample)
            {
                if (item != "")
                {
                    _textBoxList.ElementAt(index).Text = item;
                    index++;
                }
            }
		}
		private string OpenFile()
		{
			//var openFileDialog = new OpenFileDialog();
			//openFileDialog.Filter = "Audio Files|*.wav";

			var openFileDialog = new Microsoft.Win32.OpenFileDialog();
			var fileName = openFileDialog.BrowseFileToOpen("Audio Files|*.wav");

			if (!string.IsNullOrEmpty(fileName))
			{
				return fileName;
			}
			else
			{
				return "";
			}
		}

		private void OnOKBtn_Clicked(object sender, RoutedEventArgs e)
		{
			if (trialNameText.Text == "")
			{
				System.Windows.Forms.MessageBox.Show("Trial Name shouldn't be empty", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			_mushraSession.Reference = FileRef.Text;
			_mushraSession.HiddenReference = "A";
			_mushraSession.TrialName = trialNameText.Text;

            _mushraSession.Sample.Clear();
            foreach (var textBox in _textBoxList)
            {
                if (textBox.Text != "")
                {
                    _mushraSession.Sample.Add(textBox.Text);
                }
            }

			if (ValidateTrails(_mushraSession.Reference, _mushraSession.Sample))
            {
				if((_mushraSession.Sample.Any(item => (item == "Deleted") ? true : false)))
				{
					System.Windows.MessageBox.Show("The Sample(s) can't fit into the current trial because of different sampling rates as of Reference, The incorrect samples are discarded",
					"ERROR", MessageBoxButton.OK, MessageBoxImage.Error);

					while (_mushraSession.Sample.Remove("Deleted"));
				}

				var parentWindow = (Window)this.Parent;
				parentWindow.DialogResult = true;
				parentWindow.Close();
			}
		}
        public bool ValidateTrails(string referenceFiles, IList<string> sampleFiles)
        {
			var count = 0;
            using (var wavePlayer = new WavePlayer())
            {

                if (wavePlayer.CompareSampleRates(referenceFiles, sampleFiles))
                {
                    return true;
                }
                else
                {
					count++;
					return false;
				}
            }
        }

        private void OnCancelBtn_Clicked(object sender, RoutedEventArgs e)
		{
			var parentWindow = (Window)this.Parent;
			parentWindow.DialogResult = false;
			parentWindow.Close();
		}

		private void OnBrowseFiles(object sender, RoutedEventArgs e)
		{
			var button = sender as System.Windows.Controls.Button;
			var fileName = OpenFile();

			if(fileName == "")
			{
				return;
			}
			switch (button.Name)
			{
				case "BrowseBtnRef":
					FileRef.Text = fileName;
                    FileA.Text = fileName;
					EnableDisableOfBtnA();

					break;
				case "BrowseBtnA":
					FileA.Text = fileName;
					break;
				case "BrowseBtnB":
					FileB.Text = fileName;
					break;
				case "BrowseBtnC":
					FileC.Text = fileName;
					break;
				case "BrowseBtnD":
					FileD.Text = fileName;
					break;
				case "BrowseBtnE":
					FileE.Text = fileName;
					break;
				case "BrowseBtnF":
					FileF.Text = fileName;
					break;
				case "BrowseBtnG":
					FileG.Text = fileName;
					break;
				case "BrowseBtnH":
					FileH.Text = fileName;
					break;
				case "BrowseBtnI":
					FileI.Text = fileName;
					break;
				case "BrowseBtnJ":
					FileJ.Text = fileName;
					break;
				case "BrowseBtnK":
					FileK.Text = fileName;
					break;
				case "BrowseBtnL":
					FileL.Text = fileName;
					break;
			}

		}
	}
}
