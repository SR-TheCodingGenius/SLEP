using System;
using System.Configuration;
using Microsoft.Diagnostics.Tracing;

namespace SLEP.LoggerModule.Configuration.EventSources
{
	//[CLSCompliant(false)]
	public class EventSourceConfigurationElement : ConfigurationElement, IEquatable<EventSourceConfigurationElement>

	{
		/// <summary>
		/// Feature name
		/// </summary>
		[ConfigurationProperty("name", IsKey = true, IsRequired = true)]
		public string Name
		{
			get { return (string)base["name"]; }
			set { base["name"] = value; }
		}

		/// <summary>
		/// Trace level
		/// </summary>
		[ConfigurationProperty("level", IsKey = false, IsRequired = false, DefaultValue = TraceEventLevel.Verbose)]
		public TraceEventLevel Level
		{
			get { return (TraceEventLevel)base["level"]; }
			set { base["level"] = value; }
		}

		public bool Equals(EventSourceConfigurationElement other)
		{
			return Name == other.Name && Level == other.Level;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
			return Equals((EventSourceConfigurationElement)obj);
		}

		public override int GetHashCode()
		{
			var hashCode = StringComparer.OrdinalIgnoreCase.GetHashCode(Name);
			hashCode = (hashCode * 397) ^ Level.GetHashCode();
			return hashCode;
		}
	}
}
