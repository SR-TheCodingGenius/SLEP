using System;
using System.Threading;
using System.Diagnostics;
using System.Windows;

namespace SLEP.UIModule.Handlers
{

	public class MicroStopwatch : Stopwatch
	{
		readonly double _microSecPerTick =	1000000D / Frequency;

		public MicroStopwatch()
		{
			if (!IsHighResolution)
			{
				MessageBox.Show("On this system the high-resolution performance counter is not available");
				throw new Exception("On this system the high-resolution performance counter is not available");			
			}
		}

		public long ElapsedMicroseconds
		{
			get
			{
				return (long)(ElapsedTicks * _microSecPerTick);
			}
		}
	}
	class MicroTimer
	{
		public event Action<object, MicroTimerEventArgs> MicroTimerElapsed;

		Thread _threadTimer = null;
		long _ignoreEventIfLateBy = long.MaxValue;
		long _timerIntervalInMicroSec = 0;
		bool _stopTimer = true;

		public MicroTimer()
		{
		}

		public MicroTimer(long timerIntervalInMicroseconds)
		{
			Interval = timerIntervalInMicroseconds;
		}

		public long Interval
		{
			get
			{
				return Interlocked.Read(ref _timerIntervalInMicroSec);
			}
			set
			{
				Interlocked.Exchange(ref _timerIntervalInMicroSec, value);
			}
		}

		public long IgnoreEventIfLateBy
		{
			get
			{
				return System.Threading.Interlocked.Read(
					ref _ignoreEventIfLateBy);
			}
			set
			{
				System.Threading.Interlocked.Exchange(
					ref _ignoreEventIfLateBy, value <= 0 ? long.MaxValue : value);
			}
		}

		public bool Enabled
		{
			set
			{
				if (value)
				{
					Start();
				}
				else
				{
					Stop();
				}
			}
			get
			{
				return (_threadTimer != null && _threadTimer.IsAlive);
			}
		}

		public void Start()
		{
			if (Enabled || Interval <= 0)
			{
				return;
			}

			_stopTimer = false;

			ThreadStart threadStart = delegate ()
			{
				NotificationTimer(ref _timerIntervalInMicroSec, ref _ignoreEventIfLateBy, ref _stopTimer);
			};

			_threadTimer = new Thread(threadStart);
			_threadTimer.Priority = ThreadPriority.Highest;
			_threadTimer.Start();
		}

		public void Stop()
		{
			_stopTimer = true;
		}

		public void StopAndWait()
		{
			StopAndWait(System.Threading.Timeout.Infinite);
		}

		public bool StopAndWait(int timeoutInMilliSec)
		{
			_stopTimer = true;

			if (!Enabled || _threadTimer.ManagedThreadId ==	Thread.CurrentThread.ManagedThreadId)
			{
				return true;
			}

			return _threadTimer.Join(timeoutInMilliSec);
		}

		public void Abort()
		{
			_stopTimer = true;

			if (Enabled)
			{
				_threadTimer.Abort();
			}
		}

		void NotificationTimer(ref long timerIntervalInMicroSec, ref long ignoreEventIfLateBy, ref bool stopTimer)
		{
			int timerCount = 0;
			long nextNotification = 0;

			var microStopwatch = new MicroStopwatch();
			microStopwatch.Start();

			while (!stopTimer)
			{
				var callbackFunctionExecutionTime =	microStopwatch.ElapsedMicroseconds - nextNotification;

				var timerIntervalInMicroSecCurrent = Interlocked.Read(ref timerIntervalInMicroSec);
				var ignoreEventIfLateByCurrent = Interlocked.Read(ref ignoreEventIfLateBy);

				nextNotification += timerIntervalInMicroSecCurrent;
				timerCount++;
				long elapsedMicroseconds = 0;

				while ((elapsedMicroseconds = microStopwatch.ElapsedMicroseconds)	< nextNotification)
				{
					Thread.SpinWait(10);
				}

				long timerLateBy = elapsedMicroseconds - nextNotification;

				if (timerLateBy >= ignoreEventIfLateByCurrent)
				{
					continue;
				}

				MicroTimerEventArgs microTimerEventArgs =
					 new MicroTimerEventArgs(timerCount,
											 elapsedMicroseconds,
											 timerLateBy,
											 callbackFunctionExecutionTime);
				MicroTimerElapsed(this, microTimerEventArgs);
			}

			microStopwatch.Stop();
		}
	}

	public class MicroTimerEventArgs : EventArgs
	{
		// Simple counter, number times timed event (callback function) executed
		public int TimerCount { get; private set; }

		// Time when timed event was called since timer started
		public long ElapsedMicroseconds { get; private set; }

		// How late the timer was compared to when it should have been called
		public long TimerLateBy { get; private set; }

		// Time it took to execute previous call to callback function (OnTimedEvent)
		public long CallbackFunctionExecutionTime { get; private set; }

		public MicroTimerEventArgs(int timerCount,	long elapsedMicroseconds,	long timerLateBy,	long callbackFunctionExecutionTime)
		{
			TimerCount = timerCount;
			ElapsedMicroseconds = elapsedMicroseconds;
			TimerLateBy = timerLateBy;
			CallbackFunctionExecutionTime = callbackFunctionExecutionTime;
		}
	}
}
