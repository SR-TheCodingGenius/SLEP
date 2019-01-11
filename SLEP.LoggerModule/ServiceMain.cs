using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;
using Quartz;
using SLEP.LoggerModule.Configuration.EventSources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLEP.LoggerModule
{
	class ServiceMain
	{
		private static readonly log4net.ILog EventSourceLogger = log4net.LogManager.GetLogger("EventSourceLogger");
		private static IScheduler _scheduler;
		private static TraceEventSession _eventSession;
		private static FileSystemWatcher _watcher;
		private static readonly List<EventSourceConfigurationElement> EventSources = new List<EventSourceConfigurationElement>();
		private static readonly ReadOnlyDictionary<TraceEventLevel, Action<object>> EventSourceLoggers =
			new ReadOnlyDictionary<TraceEventLevel, Action<object>>(new Dictionary<TraceEventLevel, Action<object>>
			{
				[TraceEventLevel.Always] = EventSourceLogger.Info,
				[TraceEventLevel.Critical] = EventSourceLogger.Error,
				[TraceEventLevel.Error] = EventSourceLogger.Error,
				[TraceEventLevel.Warning] = EventSourceLogger.Warn,
				[TraceEventLevel.Informational] = EventSourceLogger.Info,
				[TraceEventLevel.Verbose] = EventSourceLogger.Info
			});

		static void Main(string [] args)
		{
			
		}
	}
}
