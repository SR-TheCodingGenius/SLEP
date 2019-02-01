using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace SLEP.UIModule
{
	/// <summary>
	/// Interaction logic for AboutView.xaml
	/// </summary>
	public partial class AboutView : UserControl
	{
		public AboutView()
		{
			InitializeComponent();
			label1_Copy.Content = GetBuildDate();
		}

		private string GetBuildDate()
		{
			var assembly = Assembly.GetExecutingAssembly();
			var fileInfo = new FileInfo(assembly.Location);
			return  fileInfo.LastWriteTime.ToString("dd - MMMM - yyyy");
		}

		private void OKBtn_Click(object sender, RoutedEventArgs e)
		{
			var parentWindow = (Window)this.Parent;
			parentWindow.DialogResult = true;
			parentWindow.Close();
		}
	}
}
