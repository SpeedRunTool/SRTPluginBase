namespace SRTPluginBase
{
	public class RegisteredPagesKey
	{
		/// <summary>
		/// The name of this page.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Whether this page is hidden from the UI.
		/// </summary>
		public bool Hidden { get; private set; }

		public RegisteredPagesKey(string name, bool hidden = true)
		{
			Name = name;
			Hidden = hidden;
		}

		public static implicit operator string(RegisteredPagesKey registeredPagesKey) => registeredPagesKey.Name;
		public static implicit operator RegisteredPagesKey(string name) => new RegisteredPagesKey(name);
	}
}
