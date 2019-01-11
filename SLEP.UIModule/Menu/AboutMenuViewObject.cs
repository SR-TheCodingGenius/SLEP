using SLEP.Extensions.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SLEP.UIModule.Menu
{
	public class AboutMenuViewObject
	{
		public void AboutDialog()
		{
			var window = new Window
			{
				Content = new AboutView()
			};
			window.SetWindowProperties();
			window.ShowDialog();
		}
	}
}
