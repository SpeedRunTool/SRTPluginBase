using System;

namespace SRTPluginBase
{
	/// <summary>
	/// Thrown when a required plugin cannot be found when querying an IPluginHost.
	/// </summary>
	public class PluginNotFoundException : Exception
	{
		public string PluginName { get; private set; }

		public PluginNotFoundException(string pluginName) : base() => (PluginName) = (pluginName);
	}
}
