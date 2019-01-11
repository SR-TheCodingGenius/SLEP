using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SLEP.Extensions.Handlers
{
	public static class WindowSettings
	{
		public static void SetWindowProperties(this Window window)
		{
			window.SizeToContent = SizeToContent.WidthAndHeight;
			window.ResizeMode = ResizeMode.NoResize;
			var color = (Color)ColorConverter.ConvertFromString("#E4EEF4");
			window.Background = new SolidColorBrush(color);
			window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
		}
	}
}
