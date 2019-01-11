using SLEP.Audio;
using SLEP.Extensions.Handlers;
using SLEP.Models;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Forms;

namespace SLEP.UIModule.Views
{
	/// <summary>
	/// Interaction logic for ABCtrialWindow.xaml
	/// </summary>
	/// 
	public partial class ABCtrialWindow : System.Windows.Controls.UserControl
	{
		private string _refAudioFileName;
		private string _sampleAudioFileName;
		private ABCSessionModel _abcSessionModel;
		
		[ImportingConstructor]
		public ABCtrialWindow( ABCSessionModel abcSessionModel)
		{
			InitializeComponent();
			_abcSessionModel = abcSessionModel;

            LoadCurrentTrialDetails(abcSessionModel);

        }

        private void LoadCurrentTrialDetails(ABCSessionModel abcSessionModel)
        {
            trialNumberText.Text = abcSessionModel.TrialNumber.ToString();

			trialNameText.Text = abcSessionModel.TrialName;
			
            RefFile.Text = abcSessionModel.Reference;
            SampleFile.Text = abcSessionModel.Sample;
            if (abcSessionModel.HiddenReference == "B" || abcSessionModel.HiddenReference == "")
            {
                BButton.IsChecked = true;
            }
            else
            {
                CButton.IsChecked = true;
            }
        }

		private void RefFileBrowseBtn_Click(object sender, RoutedEventArgs e)
		{
			//var openFileDialog = new System.Windows.Forms.OpenFileDialog();
			//openFileDialog.Filter = "Audio Files|*.wav";
			var openFileDialog = new Microsoft.Win32.OpenFileDialog();
			var fileName = openFileDialog.BrowseFileToOpen("Audio Files|*.wav");

			if (!string.IsNullOrEmpty(fileName))
			{
				_refAudioFileName = fileName;
				RefFile.Text = _refAudioFileName;
			}
		}

		private void SampleFileBrowseBtn_Click(object sender, RoutedEventArgs e)
		{
			//var openFileDialog = new System.Windows.Forms.OpenFileDialog();
			//openFileDialog.Filter = "Audio Files|*.wav";

			var openFileDialog = new Microsoft.Win32.OpenFileDialog();
			var fileName = openFileDialog.BrowseFileToOpen("Audio Files|*.wav");

			if (!string.IsNullOrEmpty(fileName))
			{
				_sampleAudioFileName = fileName;
				SampleFile.Text = _sampleAudioFileName;
			}
		}

		private void OkBtn_Click(object sender, RoutedEventArgs e)
		{
			if (trialNameText.Text == "")
			{
				System.Windows.Forms.MessageBox.Show("Trial Name shouldn't be blank", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			var sample = new List<string> { SampleFile.Text };
			var reference = RefFile.Text;

			if (reference == "" || sample[0] == "")
			{
				System.Windows.Forms.MessageBox.Show("Reference and Sample shouldn't be blank","Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			if (ValidateTrails(reference, sample))
			{
				_abcSessionModel.TrialName = trialNameText.Text;
				_abcSessionModel.Reference = RefFile.Text;
				_abcSessionModel.Sample = SampleFile.Text;
				_abcSessionModel.HiddenReference = BButton.IsChecked == true ? "B" : "C";

				var parentWindow = (Window)this.Parent;
				parentWindow.DialogResult = true;
				parentWindow.Close();
			}
			else
			{
				System.Windows.Forms.MessageBox.Show("Sample Rates are not same!!!!\nPlease select files of same sample rate", "ERROR", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		public bool ValidateTrails(string referenceFiles, IList<string> sampleFiles)
		{
			using (var wavePlayer = new WavePlayer())
			{
				
				if (wavePlayer.CompareSampleRates(referenceFiles, sampleFiles))
				{
					return true;
				}
				else
				{
					return false;					
				}
			}
		}

		private void CancelBtn_Click(object sender, RoutedEventArgs e)
		{
			var parentWindow = (Window)this.Parent;
			parentWindow.DialogResult = false;
			parentWindow.Close();
		}
	}
}
