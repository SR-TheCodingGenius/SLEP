using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace SLEP.LoggerModule
{
	[RunInstaller(true)]
	public sealed class LoggerServiceInstaller : ServiceInstaller
	{
		public LoggerServiceInstaller()
		{
			this.Description = "Logger Process for SLEP";
			this.ServiceName = "SLEPLoggerService";
			this.StartType = ServiceStartMode.Automatic;
		}
	}

	partial class SLEPLoggerService : ServiceBase
	{
		private static EventLog _loggerServiceEventLog;
		private readonly Action _startAction;
		private readonly Action _stopAction;
		public const string EventLogName = "Application";
		public SLEPLoggerService(Action startAction, Action stopAction)
		{
			_loggerServiceEventLog = new EventLog();
			if (!EventLog.SourceExists(this.ServiceName))
			{
				EventLog.CreateEventSource(this.ServiceName, EventLogName);
			}

			_loggerServiceEventLog.Source = this.ServiceName;
			_loggerServiceEventLog.Log = EventLogName;

			if (startAction == null)
			{
				throw new ArgumentNullException(nameof(startAction));
			}
			if (stopAction == null)
			{
				throw new ArgumentNullException(nameof(stopAction));
			}

			_startAction = startAction;
			_stopAction = stopAction;

			//InitializeComponent();
			
		}

		public static void LogEvent(string message, EventLogEntryType type)
		{
			_loggerServiceEventLog?.WriteEntry(message, type);
		}

		protected override void OnStart(string[] args)
		{
			// TODO: Add code here to start your service.
			_startAction();
		}

		protected override void OnStop()
		{
			// TODO: Add code here to perform any tear-down necessary to stop your service.
			_stopAction();
		}
	}
}
