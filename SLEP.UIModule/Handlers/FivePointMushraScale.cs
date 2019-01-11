using System;
using System.Collections.Specialized;
using System.Configuration;

namespace SLEP.UIModule.Handlers
{
	class FivePointMushraScale : IScoringScales
	{
	
		public NameValueCollection LoadScale()
		{
			var section = (NameValueCollection) ConfigurationManager.GetSection("FivePointMushScale");
			return section;
		}
	}
}
