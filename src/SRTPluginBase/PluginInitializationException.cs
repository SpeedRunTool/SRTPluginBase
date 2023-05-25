using System;

namespace SRTPluginBase
{
	/// <summary>
	/// Thrown when a required plugin cannot be initialized.
	/// </summary>
	public class PluginInitializationException : Exception
	{
		public string PluginName { get; private set; }

        public PluginInitializationException(string pluginName, string? message = default) : base(message) => (PluginName) = (pluginName);
    }
}
