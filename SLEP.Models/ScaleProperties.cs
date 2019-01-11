using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLEP.Models
{
    public class ScaleProperties
    {
		protected string _scaleName;
		protected string _scaleFilePath;
		protected bool _isUserScale;
		protected int _maxIncrement;
		protected int _minIncrement;
		protected int _rangeIncrement;

		public ScaleProperties()
		{

		}
	}
}
