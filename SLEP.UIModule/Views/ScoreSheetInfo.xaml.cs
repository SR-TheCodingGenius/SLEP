using SLEP.Logger;
using SLEP.Models;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;

namespace SLEP.UIModule
{
	/// <summary>
	/// Interaction logic for ScoreSheetInfo.xaml
	/// </summary>
	public partial class ScoreSheetInfo : System.Windows.Controls.UserControl
	{
		
		public ScoreSheetInfo()
		{
			InitializeComponent();
		}
		private static bool IsTextAllowed(string text)
		{
			Regex regex = new Regex("[^0-9]+"); //regex that matches disallowed text
			return !regex.IsMatch(text);
		}
		private bool SetScorerDetails()
		{
            if (FirstName.Text == "" || Lastname.Text == "" || AgeTb.Text == "")
            {
                System.Windows.Forms.MessageBox.Show("First name, Last name and Age are mandatory", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;

            }


            if (IsTextAllowed(AgeTb.Text) == false)
			{
				AgeTb.Clear();
				Log.Write(LogLevel.Info, "File Name: ScoreSheetInfo.xaml.cs, Method Name: SetScorerDetails(), Line Number:37 ");
				Log.Write(LogLevel.Warning, "The Age is not in valid format");
                System.Windows.Forms.MessageBox.Show("The Age should be in integer only", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
			}

            var scorerDetails = ScorerDetails.GetInstance;

			scorerDetails.FirstName = FirstName.Text;
			scorerDetails.LastName = Lastname.Text;
			scorerDetails.Age = Convert.ToInt32(AgeTb.Text);
            scorerDetails.Notes = Notes.Text;

			if (MaleBtn.IsChecked == true)
			{
                scorerDetails.Sex = "Male";
			}
			else
			{
                scorerDetails.Sex = "Female";
			}

			
			return true;
		}
		
		private void OKBtnClicked(object sender, RoutedEventArgs args)
		{
            if (SetScorerDetails() == true)
            {
                Log.Write(LogLevel.Info, "The Scorer Details are captured successfully!!!");
                var parentWindow = (Window)this.Parent;
                parentWindow.DialogResult = true;
                parentWindow.Close();
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
