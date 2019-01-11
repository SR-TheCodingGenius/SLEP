using SLEP.Models;
using System.IO;
using System.Collections.Generic;
using SLEP.Logger;
using System.Collections;

namespace SLEP.SessionAndScore.Logger.Handlers
{
	class SessionLogger : ISessionAndScoreLogger
	{
		public  string FileName { get; set; }			
		private FileStream _fileStream;
		private StreamWriter _fileWriter;
        private dynamic _modelObject = null;

		public void CloseFile()
		{
			_fileStream.Close();
			Log.Write(LogLevel.Info, "Session Logger is Closed!!!!");
		}

		public void InitializeLogger(object testDetailsObject) // add a paramter to this method as an Object
		{
			_fileStream = new FileStream(FileName, FileMode.Create);
			_fileWriter = new StreamWriter(_fileStream);

			if (testDetailsObject.GetType() == typeof(ABCTestDetailsModel))
			{
				_modelObject = (ABCTestDetailsModel) testDetailsObject;
			}
			else if(testDetailsObject.GetType() == typeof(MushraTestDetailsModel))
			{
				_modelObject = (MushraTestDetailsModel)testDetailsObject;
			}
			
			Log.Write(LogLevel.Info, "Session Logger is Initialized!!!!");
		}
	
		public void WriteFile() 
		{
			
			var sessionList = _modelObject.trials;

			_fileWriter.WriteLine("[SHURE - SLEP]");
			_fileWriter.WriteLine("Number_of_Trials:={0}", _modelObject.trials.Count);
			_fileWriter.WriteLine("Scale_Index:={0}", _modelObject.Index);
			_fileWriter.WriteLine("Scale_Selected:={0}", _modelObject.ScaleName);
			_fileWriter.WriteLine("\n");
			
			foreach (var obj in sessionList)
			{
				_fileWriter.WriteLine("[Trial {0}]", obj.TrialNumber);
				_fileWriter.WriteLine("Hidden_Reference:={0}", obj.HiddenReference);
				_fileWriter.WriteLine("Reference:={0}", obj.Reference);
				if (obj.Sample is IList)
				{
					foreach (var sample in obj.Sample)
					{
                        if (sample != "")
                        {
                            _fileWriter.WriteLine("Sample:={0}", sample);
                        }
						
					}
				}
				else
				{
					_fileWriter.WriteLine("Sample:={0}", obj.Sample);
				}
				_fileWriter.WriteLine("Trial_Name:={0}", obj.TrialName);
				_fileWriter.WriteLine("\n");
			}
			Log.Write(LogLevel.Info, "The details of the curretn session are logged succesfully");
			_fileWriter.Flush();
		}

		public void WriteHeaders()
		{
			//_fileWriter.WriteLine("[SHURE - SLEP]");
			//_fileWriter.WriteLine("Number of trials := {0}", ABCModelList.trials.Count);
			//_fileWriter.WriteLine("Scale Selected:= {0}", ABCModelList.ScaleName);
			//_fileWriter.WriteLine("\n");
			//_fileWriter.Flush();			
		}

        public void WriteMediaPool(IList<string> mediaPool)
        {
            int count = 0;
            _fileWriter.WriteLine("[MEDIA POOL]");

            foreach ( var item in mediaPool)
            {
                _fileWriter.WriteLine("File{0}:={1}", count, item);
				count++;
            }

            _fileWriter.Flush();
        }
    }
}
