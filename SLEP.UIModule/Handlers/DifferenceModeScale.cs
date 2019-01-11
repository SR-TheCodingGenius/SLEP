using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLEP.UIModule.Handlers
{
	class DifferenceModeScale : IScoringScales
	{
		public NameValueCollection LoadScale()
		{
			var section = (NameValueCollection)ConfigurationManager.GetSection("DifferenceModeScale");
			return section;
		}
	}
}
