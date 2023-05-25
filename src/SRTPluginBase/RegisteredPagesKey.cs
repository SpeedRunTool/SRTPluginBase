﻿using System;

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

		public RegisteredPagesKey(string name, bool hidden = false)
		{
			Name = name;
			Hidden = hidden;
		}

		public static implicit operator string(RegisteredPagesKey registeredPagesKey) => registeredPagesKey.Name;
        public static implicit operator RegisteredPagesKey(string name) => new RegisteredPagesKey(name);
        public static implicit operator RegisteredPagesKey(Tuple<string, bool> values) => new RegisteredPagesKey(values.Item1, values.Item2);
        public static implicit operator RegisteredPagesKey(ValueTuple<string, bool> values) => new RegisteredPagesKey(values.Item1, values.Item2);
    }
}
