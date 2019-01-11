using System;
using Microsoft.Practices.Prism.Commands;

using SLEP.Models;
using Microsoft.Practices.Prism.Regions;
using System.ComponentModel.Composition;
using Microsoft.Practices.ServiceLocation;

namespace SLEP.UIModule.Controllers
{
	[Export(typeof(UIModuleController))]
	[PartCreationPolicy(CreationPolicy.NonShared)]
	public class UIModuleController
	{
		[Import]
		public static IRegionManager _loadViewsInTransistion;
		[ImportingConstructor]
		public UIModuleController()
		{	
		
		}
		
		public void OnShowExecuted()
		{
			Uri viewNav = new Uri("/ABCTestView", UriKind.Relative);
			_loadViewsInTransistion.RequestNavigate(Regions.WorkspaceRegion, viewNav);

			Uri viewNav1 = new Uri("/ScoringScales", UriKind.Relative);
			_loadViewsInTransistion.RequestNavigate(Regions.ScoreRegion, viewNav1);

			
		}
	}
}

