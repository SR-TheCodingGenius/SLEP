using SLEP.Logger;
using SLEP.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLEP.SessionAndScore.Logger.Handlers
{
	class ScoreLogger : ISessionAndScoreLogger
	{ 
		public string FileName { get; set; }	
		private FileStream _fileStream;
		private StreamWriter _fileWriter;
        private dynamic _modelObject = null;
        private ScorerDetails _scorerDetails;
		private int trialNumber = 0;
        public void AppendFile()
		{
			throw new NotImplementedException();
		}

		public void CloseFile()
		{
			_fileStream.Close();
			trialNumber = 0;
			Log.Write(LogLevel.Info, "Score Logger is Closed!!!!");
		}

		public void InitializeLogger(object testDetailsObject)
		{
			_fileStream = new FileStream(FileName, FileMode.Create);
			_fileWriter = new StreamWriter(_fileStream);

			if (testDetailsObject.GetType() == typeof(ABCTestDetailsModel))
			{
				_modelObject = (ABCTestDetailsModel)testDetailsObject;
			}
			else if (testDetailsObject.GetType() == typeof(MushraTestDetailsModel))
			{
				_modelObject = (MushraTestDetailsModel)testDetailsObject;
			}
			_scorerDetails = ScorerDetails.GetInstance;

            Log.Write(LogLevel.Info, "Score Logger is Initialized!!!!");
		}

		public void WriteFile()
		{
			var sessionList = _modelObject.trials;
			var index = 0;
			

			var obj1 = sessionList[trialNumber];
			if (!(obj1.Sample is IList))
			{
				if (_modelObject.ScoredStimulus == obj1.HiddenReference)
				{
					_fileWriter.WriteLine("{0}\t{1}\t{2}\t({3})", obj1.TrialNumber, _modelObject.ScoredStimulus, obj1.Sample, ((Math.Ceiling(_modelObject.Score * 10)) / 10));
				}
				else
				{
					_fileWriter.WriteLine("{0}\t{1}\t{2}\t{3}", obj1.TrialNumber, _modelObject.ScoredStimulus, obj1.Sample, ((Math.Ceiling(_modelObject.Score * 10)) / 10));
				}

				Log.Write(LogLevel.Info, "The scores of the current session are logged successfully");
			}
			else if (obj1.Sample is IList)
			{
				_fileWriter.WriteLine("\n");
				var scoreIndex = 0;
				_fileWriter.WriteLine("Reference:{0}", obj1.Reference);
				foreach (var sample in obj1.Sample)
				{
					if (sample != "")
					{
						var key = index + 1;
						var scoreList = _modelObject.Score;
						var stimulusNames = _modelObject.StimulusName;

						if (scoreIndex < scoreList.Count)
						{
							_fileWriter.WriteLine("{0}\t{1}\t{2}\t{3}", obj1.TrialNumber, stimulusNames[scoreIndex], sample, (Math.Ceiling(scoreList[scoreIndex] * 10)) / 10);
						}						
					}
					scoreIndex++;
				}
				
			}
			trialNumber++;
			_fileWriter.Flush();
		}

		public void WriteHeaders()
		{
			_fileWriter.WriteLine("SHURE - SLEP Subjective Test Scores");
			_fileWriter.WriteLine("Session File := {0}", _modelObject.SessionFileName);
			_fileWriter.WriteLine("Test Method:= {0}", _modelObject.TestName);
			_fileWriter.WriteLine("Scale:= {0}", _modelObject.ScaleName);
			_fileWriter.WriteLine("\n");

			_fileWriter.WriteLine("Name:={0}\t{1}", _scorerDetails.FirstName, _scorerDetails.LastName);
			_fileWriter.WriteLine("Sex:={0}", _scorerDetails.Sex);
			_fileWriter.WriteLine("Age:={0}", _scorerDetails.Age);
			_fileWriter.WriteLine("Notes:={0}", _scorerDetails.Notes);

			_fileWriter.WriteLine("\n");

			_fileWriter.WriteLine("Trial\tFile\tScore");

			_fileWriter.Flush();

			
		}

        public void WriteMediaPool(IList<string> mediaPool)
        {
            throw new NotImplementedException();
        }
    }
}
