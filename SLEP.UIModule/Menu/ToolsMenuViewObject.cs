using SLEP.Extensions.Handlers;
using SLEP.UIModule.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SLEP.UIModule.Menu
{
	public class ToolsMenuViewObject
	{
		public void ViewScoreMergeWindow()
		{

			Window window = new Window
			{
				Title = "Score Merge",
				Content = new ScoreMerge()
			};

			window.SetWindowProperties();
			window.ShowDialog();
		}
	}
}
