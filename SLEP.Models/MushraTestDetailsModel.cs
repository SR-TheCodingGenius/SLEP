using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLEP.Models
{
	public class MushraTestDetailsModel
	{
		private static MushraTestDetailsModel _mushraModelObject;
		public static MushraTestDetailsModel GetInstance
		{
			get
			{
				if (_mushraModelObject == null)
				{
					_mushraModelObject = new MushraTestDetailsModel();
				}
				return _mushraModelObject;
			}
		}
		public IList<MushraSessionModel> trials;
		public string ScaleName = "";
		public short Index = 0;
		public string SessionFileName = "";
		public string TestName = "";
		public IList<string> StimulusName;
		public IList<double> Score;
		
	}
}
