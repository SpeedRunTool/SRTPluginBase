using System;

namespace SRTPluginBase.Interfaces
{
    public interface IPluginProducer : IPlugin, IEquatable<IPluginProducer>
    {
        /// <summary>
        /// Requests the producer plugin to return new data.
        /// </summary>
        object? Refresh();

        public new bool Equals(IPluginProducer? other) => (this as IPlugin).Equals(other);

        public new int GetHashCode() => (this as IPlugin).GetHashCode();
    }
}
