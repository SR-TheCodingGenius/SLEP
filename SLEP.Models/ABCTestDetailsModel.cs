using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace SLEP.Models
{
	public  class ABCTestDetailsModel : EventArgs
	{
        private static ABCTestDetailsModel _abcModelObject;
        public static ABCTestDetailsModel GetInstance
        {
            get
            {
                if (_abcModelObject == null)
                {
                    _abcModelObject = new ABCTestDetailsModel();
                }
				return _abcModelObject;
			}
        }

        public  IList<ABCSessionModel> trials;
		public string ScaleName { get; set; } = "";
		public short Index { get; set; } = 0;
		public string SessionFileName { get; set; } = "";
		public string TestName { get; set; } = "";
		public double Score { get; set; } = 0.0;
		public string ScoredStimulus { get; set; } = "";
	
	}

	public class ScorerDetails
	{
        private static ScorerDetails _scorerDetailsObject;
        public static ScorerDetails GetInstance
        {
            get
            {
                if (_scorerDetailsObject == null)
                {
                    _scorerDetailsObject = new ScorerDetails();

                }
				return _scorerDetailsObject;
			}

        }

        public  string FirstName = "";
		public  string LastName = "";
		public int Age = 0;
		public  string Sex = "";
		public  string Notes = "";
		public  string ScoreFileName = "";		
	}
}
