using Microsoft.Practices.Prism.Commands;

using SLEP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLEP.UIModule
{
	public interface IUIModuleController
	{
		DelegateCommand<ViewObject> ShowViewCommand { get; }
	
	}
}
