using System;
using System.Threading.Tasks;

namespace SRTPluginBase
{
	public static class Helpers
	{
		public static object? ConvertDBNullToNull(object? value) => value is not DBNull ? value : null;

        /// <summary>
        /// Allows you to execute an action on *Changed events of Mud components.
        /// </summary>
        /// <typeparam name="T">The type that the Mud component's value represents.</typeparam>
        /// <param name="action">The action to execute when the value changes.</param>
        /// <param name="name">The name of the property. This value is supplied to the action.</param>
        /// <param name="value">The new value from the component. This value is supplied to the action.</param>
        /// <param name="cascadingStateChanger">(Optional) <seealso cref="CascadingStateChanger"/>? instance to trigger state changes in other components.</param>
        /// <example>&lt;MudCheckBox T="bool" Checked=pluginHost.HostConfiguration.ShowDebugMenu CheckedChanged="(value) => SRTPluginBase.Helpers.MudSettingChanged(SettingChanged, nameof(pluginHost.HostConfiguration.ShowDebugMenu), value, cascadingStateChanger)" Color="Color.Secondary"&gt;Show Debug Menu&lt;/MudCheckBox&gt;</example>
        /// <returns>A Task representing the completion of the operation.</returns>
        public static Task MudSettingChanged<T>(Action<string, T> action, string name, T value, CascadingStateChanger? cascadingStateChanger = default)
        {
            action.Invoke(name, value);
            cascadingStateChanger?.NotifyStateChanged(); // Allows us to re-render another component (NavMenu) from this component. StateHasChanged alone does not allow us to do that.
            return Task.CompletedTask;
        }
    }
}
