using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLEP.Models
{
	public class TrailsModel
	{
		public string _trialName;
		public string _trialType;
		public IDictionary<int, IDictionary<Stream, IList<Stream>>> _trail;
		public IList<int> _trailWindow;
		public string _hiddenReference;
	}
}
