using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLEP.LoggerModule.Configuration.EventSources
{
	[ConfigurationCollection(typeof(EventSourceConfigurationElement))]
	public class EventSourceConfigurationElementCollection : ConfigurationElementCollection
	{
		protected override ConfigurationElement CreateNewElement()
		{
			return new EventSourceConfigurationElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((EventSourceConfigurationElement)element).Name;
		}
	}
}
