using SLEP.BootStrapper;
using SLEP.Logger;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace SLEP
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
	
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			// Configure Bootstrapper
			var bootstrapper = new SlepBootStrapper();
			bootstrapper.Run();

			AppDomain.CurrentDomain.UnhandledException += LogUnhandledException;
			this.Dispatcher.UnhandledException += LogUnhandledException;
			TaskScheduler.UnobservedTaskException += LogUnhandledException;
		}

		private void LogUnhandledException(object sender, UnobservedTaskExceptionEventArgs e)
		{
			var exceptionObject = e.Exception as Exception;

			if (exceptionObject == null)
			{
				Log.Write(LogLevel.Error, exceptionObject.Message);
				return;
			}

			Log.Write(LogLevel.Error, exceptionObject.Message, exceptionObject);
			//MessageBox.Show(exceptionObject.Message);
			e.SetObserved();
		}

		private void LogUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			var exceptionObject = e.ExceptionObject as Exception;

			if (exceptionObject == null)
			{
				Log.Write(LogLevel.Error, exceptionObject.Message);
				return;
			}
			//MessageBox.Show(exceptionObject.Message);
			Log.Write(LogLevel.Error, exceptionObject.Message, exceptionObject);

		}

		private void LogUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			var exceptionObject = e.Exception as Exception;

			if (exceptionObject == null)
			{
				Log.Write(LogLevel.Error, exceptionObject.Message);
				return;
			}
			//MessageBox.Show(exceptionObject.Message);
			Log.Write(LogLevel.Error, exceptionObject.Message, exceptionObject);
			e.Handled = true;
		}
		
	}
}
