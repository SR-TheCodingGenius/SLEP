using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLEP.Models
{
	public class AudioServiceModel
	{
		TimeSpan TotalDuration { get; set; }
		TimeSpan AudioCurrentPosition { get; set; }
		TimeSpan AudioStartTime { get; set; }
		TimeSpan AudioStopTime { get; set; }
	}
}
