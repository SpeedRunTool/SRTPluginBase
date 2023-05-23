using System;

namespace SRTPluginBase
{
    public interface IPluginProducer<T> : IPlugin, IEquatable<IPluginProducer<T>>
    {
        /// <summary>
        /// Requests the producer plugin to update the Data property.
        /// </summary>
        void Refresh();

        /// <summary>
        /// The most recent plugin-specific data structure for this producer.
        /// </summary>
        T? Data { get; }

        /// <summary>
        /// The DateTime representing the last time the Data property was updated. This value is null if the Data property has not been populated.
        /// </summary>
        DateTime? LastUpdated { get; }

        public new bool Equals(IPluginProducer<T>? other) => (this as IPlugin).Equals(other);

        public new int GetHashCode() => (this as IPlugin).GetHashCode();
    }
}
