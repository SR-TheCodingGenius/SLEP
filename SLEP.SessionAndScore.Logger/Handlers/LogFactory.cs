using SLEP.Logger;
using SLEP.SessionAndScore.Logger.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLEP.SessionAndScore.Logger.Handlers
{
	public class LogFactory 
	{
		public static ISessionAndScoreLogger GetLoggerObject(byte loggerType)
		{
			ISessionAndScoreLogger loggerObject = null;
			
			if (loggerType == (byte)(LoggerType.SessionLogger))
			{
				loggerObject = new SessionLogger();
			}
			else if(loggerType == (byte)(LoggerType.ScoreLogger))
			{
				loggerObject = new ScoreLogger();
			}
			
			return loggerObject;
		}
	}
}
