using log4net;
using System;
using System.Collections.Generic;
using log4net.Config;

namespace SLEP.Logger
{
	public static class Logger
	{
		private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static Dictionary<LogLevel, Action<string>> _actions;
		private static Dictionary<LogLevel, Action<string, Exception>> _exceptionActions;

		static Logger()
		{
			XmlConfigurator.Configure();
			_actions = new Dictionary<LogLevel, Action<string>>();
			_actions.Add(LogLevel.Debug, WriteDebug);
			_actions.Add(LogLevel.Error, WriteError);
			_actions.Add(LogLevel.Fatal, WriteFatal);
			_actions.Add(LogLevel.Info, WriteInfo);
			_actions.Add(LogLevel.Warning, WriteWarning);

			_exceptionActions = new Dictionary<LogLevel, Action<string, Exception>>();
			_exceptionActions.Add(LogLevel.Error, WriteError);
			_exceptionActions.Add(LogLevel.Fatal, WriteFatal);

		}

		public static void Write(LogLevel level, string message)
		{
			if (!string.IsNullOrEmpty(message))
			{
				if (level > LogLevel.Warning || level < LogLevel.Debug)
					throw new ArgumentOutOfRangeException("level");

				// Now call the appropriate log level message.
				_actions[level](message);
			}
		}

		public static void Write(LogLevel level, string message, Exception exception)
		{
			if (!string.IsNullOrEmpty(message))
			{
				if (level > LogLevel.Warning || level < LogLevel.Debug)
					throw new ArgumentOutOfRangeException("level");

				// Now call the appropriate log level message.
				_exceptionActions[level](message, exception);
			}
		}

		private static void WriteDebug(string message)
		{
			if (_logger.IsDebugEnabled)
				_logger.Debug(message);
		}

		private static void WriteError(string message)
		{
			if (_logger.IsErrorEnabled)
				_logger.Error(message);
		}

		private static void WriteError(string message, Exception exception)
		{
			if (_logger.IsErrorEnabled)
				_logger.Error(message, exception);
		}

		private static void WriteFatal(string message)
		{
			if (_logger.IsFatalEnabled)
				_logger.Fatal(message);
		}

		private static void WriteFatal(string message, Exception exception)
		{
			if (_logger.IsFatalEnabled)
				_logger.Fatal(message, exception);
		}

		private static void WriteInfo(string message)
		{
			if (_logger.IsInfoEnabled)
				_logger.Info(message);
			

		}

		private static void WriteWarning(string message)
		{
			if (_logger.IsWarnEnabled)
				_logger.Warn(message);
		}
	}

	public enum LogLevel
	{
		Debug = 0,
		Error = 1,
		Fatal = 2,
		Info = 3,
		Warning = 4
	}
}
