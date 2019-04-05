using Microsoft.Practices.Prism.MefExtensions;
using SLEP.Views;
using System.Windows;
using Microsoft.Practices.Prism.Modularity;
using System.ComponentModel.Composition.Hosting;
using SLEP.Infrastructure.Behaviours;
using Microsoft.Practices.Prism.Regions;
using System.ComponentModel.Composition;
using SLEP.Models;
using SLEP.UIModule.Controllers;

namespace SLEP.BootStrapper
{

	public class SlepBootStrapper : MefBootstrapper
	{
		private static CompositionContainer _container;
		protected override DependencyObject CreateShell()
		{
			_container = this.Container;
			return this.Container.GetExportedValue<ShellWindow>();
			
		}

		protected override void ConfigureContainer()
		{
			base.ConfigureContainer();
			this.Container.ComposeExportedValue(new UIModuleController());
		}

		protected override void InitializeShell()
		{
			base.InitializeShell();
			Application.Current.MainWindow = (ShellWindow)this.Shell;
			Application.Current.MainWindow.Show();
		}
		protected override void ConfigureAggregateCatalog()
		{
			this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(SlepBootStrapper).Assembly));
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
		public static CompositionContainer GetContainer
		{
			get
			{
				return _container;
			}
		}

	}
}
