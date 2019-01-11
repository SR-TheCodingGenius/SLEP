using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLEP.Models
{
	public static class ABCModelList
	{
		public static IList<ABCSessionModel> trails;
		public static string ScaleName;
		public static short Index;
		public static string SessionFileName;
		public static string TestName;
		public static IList<double> Score;

	}

	public static class ScorerDetails
	{
		public static string FirstName;
		public static string LastName;
		public static int Age;
		public static string Sex;
		public static string Notes;
		public static string ScoreFileName;		
	}
}
