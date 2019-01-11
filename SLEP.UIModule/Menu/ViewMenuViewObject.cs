using SLEP.Extensions.Handlers;
using SLEP.UIModule.Views;
using System.Windows;

namespace SLEP.UIModule.Menu
{
	public class ViewMenuViewObject
	{	
		public void ViewScoreSheet()
		{
		
			
			Window window = new Window
			{
				
				Content = new ViewScores()
			};

			window.SetWindowProperties();
			window.ShowDialog();
		}
		
	}
}
