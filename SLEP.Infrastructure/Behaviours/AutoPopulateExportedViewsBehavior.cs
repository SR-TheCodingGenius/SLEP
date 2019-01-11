using Microsoft.Practices.Prism.Regions;
using SLEP.Infrastructure.Behaviours;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLEP.Infrastructure.Behaviours
{
	[Export(typeof(AutoPopulateExportedViewsBehavior))]
	[PartCreationPolicy(CreationPolicy.NonShared)]
	public class AutoPopulateExportedViewsBehavior : RegionBehavior, IPartImportsSatisfiedNotification
	{
		public void OnImportsSatisfied()
		{
			AddRegisteredViews();
		}

		protected override void OnAttach()
		{
			AddRegisteredViews();
		}


		private void AddRegisteredViews()
		{
			if (this.Region != null)
			{
				foreach (var viewEntry in this.RegisteredViews)
				{
					if (viewEntry.Metadata.RegionName == this.Region.Name)
					{
						var view = viewEntry.Value;

						if (!this.Region.Views.Contains(view))
						{
							this.Region.Add(view);
						}
					}
				}
			}
		}

		[ImportMany(AllowRecomposition = true)]
		Lazy<object, IViewRegionRegistration>[] RegisteredViews { get; set; }
	}
}
