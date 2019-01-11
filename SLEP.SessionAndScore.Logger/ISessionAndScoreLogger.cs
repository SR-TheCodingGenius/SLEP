using SLEP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLEP.SessionAndScore.Logger
{
	public interface ISessionAndScoreLogger
	{	
		string FileName { get; set;}
		void InitializeLogger(object testModelObject);

		void WriteHeaders();
		void WriteFile();
		void CloseFile();

        void WriteMediaPool(IList<string> mediaPool);
	}
	
}
