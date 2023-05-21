using System;

namespace SRTPluginBase
{
	public static class Helpers
	{
		public static object? ConvertDBNullToNull(object? value) => value is not DBNull ? value : null;
	}
}
