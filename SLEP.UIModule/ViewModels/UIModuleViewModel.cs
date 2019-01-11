using Microsoft.Practices.Prism.ViewModel;

using System.ComponentModel.Composition;
using System.Windows.Input;

namespace SLEP.UIModule.ViewModels
{
	
	public class UIModuleViewModel : NotificationObject
	{
	
		public UIModuleViewModel(IUIModuleController uiModuleController)
		{
			ShowView = uiModuleController.ShowViewCommand;
		}

		private ICommand showView;

		public ICommand ShowView
		{
			get { return showView; }
			set
			{
				if (showView != value)
				{
					showView = value;
					RaisePropertyChanged("ShowView");
				}
			}
		}
	}
}
