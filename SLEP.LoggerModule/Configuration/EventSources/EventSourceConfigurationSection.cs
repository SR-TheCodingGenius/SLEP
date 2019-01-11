using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLEP.LoggerModule.Configuration.EventSources
{
	public class EventSourceConfigurationSection : ConfigurationSection
	{
		/// <summary>
		/// Enabled
		/// </summary>
		[ConfigurationProperty("enabled", IsRequired = true, DefaultValue = true)]
		public bool Enabled
		{
			get { return (bool)base["enabled"]; }
			set { base["enabled"] = value; }
		}

		/// <summary>
		/// Feature collection
		/// </summary>
		[ConfigurationProperty("providers")]
		public EventSourceConfigurationElementCollection EventSourceConfigurationElementCollection => (EventSourceConfigurationElementCollection)base["providers"];

		/// <summary>
		/// Providers List
		/// </summary>
		public IReadOnlyList<EventSourceConfigurationElement> Providers
			=> EventSourceConfigurationElementCollection.Cast<EventSourceConfigurationElement>().ToList().AsReadOnly();
	}
}
