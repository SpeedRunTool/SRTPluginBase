using System;
using System.Threading.Tasks;

namespace SRTPluginBase
{
	public static class Helpers
	{
		public static object? ConvertDBNullToNull(object? value) => value is not DBNull ? value : null;

        public static Task MudSettingChanged(CascadingStateChanger cascadingStateChanger, Action action)
        {
            action.Invoke();
            cascadingStateChanger.NotifyStateChanged(); // Allows us to re-render another component (NavMenu) from this component. StateHasChanged alone does not allow us to do that.
            return Task.CompletedTask;
        }
    }
}
