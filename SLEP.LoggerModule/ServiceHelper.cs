using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SLEP.LoggerModule
{
	class ServiceHelper<T> : ServiceBase
	{
		private const string Log4NetConfigFile = "log4Net.config";
		private readonly string _serviceName;

		public ServiceHelper(string serviceName)
		{
			_serviceName = serviceName;
		}

		public bool IsInstalled()
		{
			using (var serviceController = new ServiceController(_serviceName))
			{
				try
				{
					var serviceStatus = serviceController.Status;
				}
				catch
				{
					return false;
				}
			}

			return true;
		}

		public bool IsRunning()
		{
			using (var serviceController = new ServiceController(_serviceName))
			{

				if(!IsInstalled())
				{
					return false;
				}
				return (serviceController.Status == ServiceControllerStatus.Running);
			}
		}

		private AssemblyInstaller GetInstaller()
		{
			var installer = new AssemblyInstaller(typeof(T).Assembly, null) { UseNewContext = true };
			return installer;
		}

		public void Install(string logFilesPath)
		{
			
			if (IsInstalled())
			{
				return;
			}

			ConfigureLogFilesPath(logFilesPath);


			using (AssemblyInstaller installer = GetInstaller())
			{
				IDictionary state = new Hashtable();
				try
				{
					installer.Install(state);
					installer.Commit(state);
				}
				catch
				{
					try
					{
						installer.Rollback(state);
					}
					catch
					{
						// ignored
					}
					throw;
				}
			}
		}

		public void Uninstall(bool cleanup)
		{
			if (!IsInstalled())
			{
				return;
			}

			Stop();
			using (AssemblyInstaller installer = GetInstaller())
			{
				IDictionary state = new Hashtable();
				installer.Uninstall(state);
			}

		}

		public void Restart()
		{
			Stop();
			Start();
		}

		public void Start()
		{
			Console.WriteLine("Processing Start command");
			if (!IsInstalled())
			{
				return;
			}

			using (var controller = new ServiceController(_serviceName))
			{
				if (controller.Status == ServiceControllerStatus.Running)
				{
					return;
				}
				controller.Start();
				try
				{
					// Starting the logger service can take a while on some PCs. 
					// The current assumption is that it's the querying all performance counter categories
					// in the system is what can take a while to complete.
					controller.WaitForStatus(ServiceControllerStatus.Running,
						TimeSpan.FromMinutes(1));
				}
				catch (System.ServiceProcess.TimeoutException)
				{
				}
			}
		}
		public new void Stop()
		{
			if (!IsInstalled())
			{
				return;
			}

			using (var controller = new ServiceController(_serviceName))
			{
				if (controller.Status == ServiceControllerStatus.Stopped)
				{
					return;
				}

				controller.Stop();
				controller.WaitForStatus(ServiceControllerStatus.Stopped,
					TimeSpan.FromSeconds(20));

				// Ensure that the process has stopped before returning
				StopProcess();
			}
		}
		private static void StopProcess()
		{
			// Get all processes of our "own" kind except for our self
			var currentProcess = Process.GetCurrentProcess();
			var processes = Process.GetProcessesByName(currentProcess.ProcessName).Where(p => p.Id != currentProcess.Id).ToList();

			// Kill all processes of "our" kind in the system.
			// Maybe we ought to only kill the process that belongs to the service, but this is how it do it for now.
			// (Getting the PID of the service is described here: http://stackoverflow.com/questions/23084720/get-the-pid-of-a-windows-service)

			var tasks = new List<Task>();
			processes.ForEach(p => tasks.Add(Task.Run(() =>
			{
				if (p.HasExited)
				{
					return;
				}

				// Allow a grace periode for the process to make it's exit voluntarily (The SCM has just reported the service as stopped)
				if (p.WaitForExit(10000))
				{
					return;
				}

				// It didn't happen, going for the kill
				p.Kill();

				if (p.HasExited)
				{
					return;
				}

				if (!p.WaitForExit(10000))
				{
					throw new System.ServiceProcess.TimeoutException($"Timed out waiting for process '{p.Id} ({p.ProcessName})' to exit.");
				}
			})));

			Task.WaitAll(tasks.ToArray());
		}

		private static void ConfigureLogFilesPath(string logFilesPath)
		{
			if (string.IsNullOrWhiteSpace(logFilesPath))
			{
				Console.WriteLine("Warning: Log files path not specified, using defaults");
				return;
			}

			Console.WriteLine($"Setting log files path to: {logFilesPath}");

			var xmlDoc = new XmlDocument();
			xmlDoc.Load(Log4NetConfigFile);

			// For now setting all appenders with a file property to write to same path
			var nodes = xmlDoc.SelectNodes("log4net/appender/file/@value");
			if (nodes != null)
			{
				foreach (XmlNode node in nodes)
				{
					node.Value = logFilesPath;
				}
				xmlDoc.Save(Log4NetConfigFile);
			}
		}

	}
}
