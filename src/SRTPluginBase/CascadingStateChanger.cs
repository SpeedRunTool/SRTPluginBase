using System;

namespace SRTPluginBase
{
    public class CascadingStateChanger
    {
        public event Action? OnChange;

        public void NotifyStateChanged() => OnChange?.Invoke();
    }
}
