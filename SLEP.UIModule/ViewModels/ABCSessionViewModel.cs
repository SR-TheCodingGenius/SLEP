using SLEP.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SLEP.UIModule.ViewModels
{
	public class ABCSessionViewModel : ABCSessionModel, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		public int _TrailNumber
		{
			get { return TrailNumber; }
			set { TrailNumber = value; OnPropertyChanged(); }

		}

		public string _TrailName
		{
			get { return TrialName; }
			set { TrialName = value; OnPropertyChanged(); }
		}

		public string _ReferenceFile
		{
			get { return Reference; }
			set { Reference = value; OnPropertyChanged(); }
		}

		public string _SampleFile
		{
			get { return Sample; }
			set { Sample = value; OnPropertyChanged(); }
		}

		public string _HiddenRefBorC
		{
			get { return ReferenceBorC; }
			set { ReferenceBorC = value; OnPropertyChanged(); }
		}

		public void OnPropertyChanged([CallerMemberName] string caller = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
		}
	}
}
