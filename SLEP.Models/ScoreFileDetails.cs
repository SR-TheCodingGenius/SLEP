using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SLEP.Models
{
	public class ScoreFileDetails
	{
		private string _sessionFileName;
		private string _testName;
		private string _scaleName;
		private string _testerName;
		private string _sex;
		private string _age;
		private string _notes;

		public string SessionName { get { return _sessionFileName; } set { _sessionFileName = value; } }
		public string TestName { get { return _testName; } set { _testName = value; } }
		public string TesterName { get { return _testerName; } set { _testerName = value; } }
		public string ScaleName { get { return _scaleName; } set { _scaleName = value; } }
		public string Sex { get { return _sex; } set { _sex = value; } }
		public string Age { get { return _age; } set { _age = value; } }
		public string TesterNotes { get { return _notes; } set { _notes = value; } }

		public List<ScoreDetails> ScoresList;
	}
	public class ScoreDetails : INotifyPropertyChanged
	{
		//MODEL
		private string _trial;
		private string _button;
		private string _file;
		private string _score;
		

		// VIEW MODEL
		public string Trial { get { return _trial; } set { _trial = value; OnPropertyChanged(); } }
		public string Button { get { return _button; } set { _button = value; OnPropertyChanged(); } }
		public string File { get { return _file; } set { _file = value; OnPropertyChanged(); } }
		public string Score { get { return _score; } set { _score = value; OnPropertyChanged(); } }

		
		public event PropertyChangedEventHandler PropertyChanged;

		// NOTIFY VIEW WHEN MODEL CHANGED
		public void OnPropertyChanged([CallerMemberName] string caller = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
		}
	}

	
}
