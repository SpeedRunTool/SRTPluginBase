using System;
using System.Collections;
using System.Collections.Generic;

namespace SRTPluginBase
{
	public class RegisteredPagesKeyComparer : IComparer<RegisteredPagesKey>, IEqualityComparer<RegisteredPagesKey>, IComparer, IEqualityComparer
	{
		public StringComparison NameStringComparison { get; set; }

		public RegisteredPagesKeyComparer(StringComparison nameStringComparison) => (NameStringComparison) = (nameStringComparison);

		public readonly static RegisteredPagesKeyComparer DefaultComparer = new RegisteredPagesKeyComparer(StringComparison.OrdinalIgnoreCase);

		public int Compare(RegisteredPagesKey x, RegisteredPagesKey y) => string.Compare(x.Name, y.Name, System.StringComparison.OrdinalIgnoreCase);
		public int Compare(object x, object y) => Compare((RegisteredPagesKey)x, (RegisteredPagesKey)y);

		public bool Equals(RegisteredPagesKey x, RegisteredPagesKey y) => string.Equals(x.Name, y.Name, System.StringComparison.OrdinalIgnoreCase);
		public new bool Equals(object x, object y) => Equals((RegisteredPagesKey)x, (RegisteredPagesKey)y);

		public int GetHashCode(RegisteredPagesKey obj) => obj.Name.GetHashCode();
		public int GetHashCode(object obj) => GetHashCode((RegisteredPagesKey)obj);
	}
}
