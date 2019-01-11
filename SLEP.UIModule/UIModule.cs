using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using SLEP.Logger;
using SLEP.UIModule.Controllers;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLEP.UIModule
{
	[ModuleExport(typeof(UIModule))]
	class UIModule : IModule
	{
		[ImportingConstructor]
		public UIModule(IRegionManager regionManager)
		{
			UIModuleController._loadViewsInTransistion = regionManager;
		}
		public void Initialize()
		{

			Log.Write(LogLevel.Info, "The UI Module is Initialized Successfully");
			var uiController = new UIModuleController();
			uiController.OnShowExecuted();
		}
	}
}
