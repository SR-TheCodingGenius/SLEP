using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;
using SLEP.UIModule;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SLEP.ViewModels
{
	[Export(typeof(ShellWindowViewModel))]
	[PartCreationPolicy(CreationPolicy.NonShared)]
	class ShellWindowViewModel 
	{
		

		[ImportingConstructor]
		public ShellWindowViewModel()
		{
			_exitCommand = new DelegateCommand(OnExit);
		}

		public ICommand _exitCommand { get; private set; }
		private void OnExit()
		{
			Application.Current.Shutdown();
		}
		
	}
}
