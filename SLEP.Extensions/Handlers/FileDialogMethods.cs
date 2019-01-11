using Microsoft.Win32;
using System.Collections.Generic;

namespace SLEP.Extensions.Handlers
{
	public static class FileDialogMethods
	{
		public static string BrowseFileToOpen(this OpenFileDialog fileObj, string typeOfFile)
		{
			var openFileDialog = fileObj;
			string fileName = "";
			openFileDialog.Filter = typeOfFile;

			var result = openFileDialog.ShowDialog();
			if (result == true)
			{
				fileName = openFileDialog.FileName;
			}
			else
			{
				fileName = "";
			}

			return fileName;
		}

		public static IList<string> BrowseMultipleFilesToOpen(this OpenFileDialog fileObj, string typeOfFile)
		{
			var openFileDialog = fileObj; 
			IList<string> fileNameList = null;
			openFileDialog.Filter = typeOfFile;
			openFileDialog.Multiselect = true;

			var result = openFileDialog.ShowDialog();
			if (result == true)
			{
				fileNameList = openFileDialog.FileNames;
			}
			

			return fileNameList;
		}


	}
}
