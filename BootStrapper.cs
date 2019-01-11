using Microsoft.Practices.Prism.MefExtensions;
using SLEP.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Practices.Prism.Modularity;
using System.ComponentModel.Composition.Hosting;
using SLEP.Infrastructure.Behaviours;
using Microsoft.Practices.Prism.Regions;
using System.ComponentModel.Composition;

namespace SLEP
{
	class BootStrapper : MefBootstrapper
	{
		
		protected override DependencyObject CreateShell()
		{
			
			return this.Container.GetExportedValue<ShellWindow>();
		}
		protected override void InitializeShell()
		{
			base.InitializeShell();

			Application.Current.MainWindow = (ShellWindow)this.Shell;
			Application.Current.MainWindow.Show();

		}
		protected override void ConfigureAggregateCatalog()
		{
			this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(BootStrapper).Assembly));
			this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(AutoPopulateExportedViewsBehavior).Assembly));

		}

		protected override IRegionBehaviorFactory ConfigureDefaultRegionBehaviors()
		{
			var factory = base.ConfigureDefaultRegionBehaviors();
			factory.AddIfMissing("AutoPopulateExportedViewsBehavior", typeof(AutoPopulateExportedViewsBehavior));
			return factory;
		}
		protected override IModuleCatalog CreateModuleCatalog()
		{
			// When using MEF, the existing Prism ModuleCatalog is still the place to configure modules via configuration files.
			return new ConfigurationModuleCatalog();
		}
	}
}
