using SLEP.Logger;
using SLEP.UIModule.Handlers.Enums;

namespace SLEP.UIModule.Handlers
{
	class ScaleFactory
	{
		public static IScoringScales GetScalesObject(short scaleType, string scaleName)
		{

			IScoringScales scaleObject = null;

			switch (scaleType)
			{
				case (short)ScaleType.FivePointMushraScale:
					scaleObject = new FivePointMushraScale();
					break;

				case (short)ScaleType.FivePointBS1116Scale:
					scaleObject = new FivePointBS1116Scale();
					break;

				case (short) ScaleType.SevenPointBS562Scale:
					scaleObject = new SevenPointBS562Scale();
					break;				

				case (short)ScaleType.DifferenceModeScale:
					scaleObject = new DifferenceModeScale();
					break;

				
				default:
					scaleObject = new CustomScale(scaleName);
					break;
			}
			Log.Write(LogLevel.Info, "The Scoring Scale Object constructed successfully, based on user selection");
			
			return scaleObject;
		}
		
	}
}
