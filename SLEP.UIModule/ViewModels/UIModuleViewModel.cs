using Microsoft.Practices.Prism.Mvvm;
using System.Windows.Input;

namespace SLEP.UIModule.ViewModels
{

	public class UIModuleViewModel : BindableBase
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
					OnPropertyChanged("ShowView");
				}
			}
		}
	}
}
