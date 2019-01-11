using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace SLEP.Models
{
	[Serializable]
	public class CustomScaleProperties
	{
		public string ScaleName { get; set; }
		public List<string> CustomScaleLabels { get; set; } = new List<string>();
		public List<string> CustomComments { get; set; } = new List<string>();
	}
}
