using System;
using System.Collections.Specialized;
using System.Configuration;

namespace SLEP.UIModule.Handlers
{
	class SevenPointBS562Scale : IScoringScales
	{
		
		public NameValueCollection LoadScale()
		{
			var section = (NameValueCollection)ConfigurationManager.GetSection("SevenPointBS562Scale");
			return section;
		}
	}
}
