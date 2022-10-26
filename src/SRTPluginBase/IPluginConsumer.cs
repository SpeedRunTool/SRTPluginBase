using System;

namespace SRTPluginBase
{
    public interface IPluginConsumer : IPlugin, IEquatable<IPluginConsumer>
    {
        public new bool Equals(IPluginConsumer? other) => (this as IPlugin).Equals(other);

        public new int GetHashCode() => (this as IPlugin).GetHashCode();
    }
}
