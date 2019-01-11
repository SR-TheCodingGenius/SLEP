using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SLEP.Models
{
	public class TrialList : INotifyPropertyChanged
	{
		// MODEL
		private int _trialNumber;
		private string _trialName;

		// VIEW MODEL
		public int TrialNumber { get { return _trialNumber; } set { _trialNumber = value; OnPropertyChanged(); } }
		public string TrialName { get { return _trialName; } set { _trialName = value; OnPropertyChanged(); } }

		public event PropertyChangedEventHandler PropertyChanged;

		// NOTIFY VIEW WHEN MODEL CHANGED
		public void OnPropertyChanged([CallerMemberName] string caller = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
		}
	}
}
