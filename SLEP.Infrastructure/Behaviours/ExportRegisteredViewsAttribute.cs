using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLEP.Infrastructure.Behaviours
{
	
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	[MetadataAttribute]
	public class ExportRegisteredViewsAttribute : ExportAttribute, IRegionRegisteredViews
	{
		public ExportRegisteredViewsAttribute()
			: base(typeof(object))
		{ }

		public ExportRegisteredViewsAttribute(string viewName)
			: base(viewName, typeof(object))
		{ }

		public string RegionName { get; set; }
	}
}
