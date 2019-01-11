using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SLEP.Models
{
	/// <summary>
	/// Each instance of this class holds the trial information of of ABC Session 
	/// and also notfies the Session Grid of ABC Session when the model is changed.
	/// </summary>
	public class ABCSessionModel : INotifyPropertyChanged
    {
		// MODEL
		private int _trialNumber;
		private string _reference = "";
		private string _sample = "";
		private string _hiddenRef = "";
		private string _trialName = "";
		
		
		// VIEW MODEL
		public int TrialNumber { get { return _trialNumber; } set { _trialNumber = value;  OnPropertyChanged(); } }
		public string Reference { get { return _reference; } set { _reference = value; OnPropertyChanged(); } }
		public string Sample{ get {return _sample; } set { _sample = value;  OnPropertyChanged(); } }
		public string HiddenReference { get { return _hiddenRef; } set { _hiddenRef = value;  OnPropertyChanged(); } }
		
		public string  TrialName { get {return _trialName; } set { _trialName = value;  OnPropertyChanged(); } }

		public event PropertyChangedEventHandler PropertyChanged;

		// NOTIFY VIEW WHEN MODEL CHANGED
		public void OnPropertyChanged([CallerMemberName] string caller = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
		}

     
    }
}
