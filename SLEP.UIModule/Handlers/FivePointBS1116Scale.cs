using System;
using System.Collections.Specialized;
using System.Configuration;

namespace SLEP.UIModule.Handlers
{
	class FivePointBS1116Scale : IScoringScales
	{
		public NameValueCollection LoadScale()
		{
			var section = (NameValueCollection)ConfigurationManager.GetSection("FivePointBS1116Scale");
			return section;
		}

	}
}
