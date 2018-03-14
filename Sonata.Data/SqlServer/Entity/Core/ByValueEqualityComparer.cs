#region Namespace Sonata.Data.SqlServer.Entity.Core
//	TODO
#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace Sonata.Data.SqlServer.Entity.Core
{
	public class ByValueEqualityComparer : IEqualityComparer<object>
	{
		#region Members

		internal static readonly ByValueEqualityComparer Default = new ByValueEqualityComparer();

		#endregion

		#region Constructors

		private ByValueEqualityComparer()
		{
		}

		#endregion

		#region Methods

		#region IEqualityComparer<T> Members

		public int GetHashCode(object obj)
		{
			if (obj == null)
				return 0;

			return obj is byte[] bytes ? ComputeBinaryHashCode(bytes) : obj.GetHashCode();
		}

		#endregion

		#region Object Members

		public new bool Equals(object x, object y)
		{
			if (object.Equals(x, y))
				return true;

			if (x is byte[] first && y is byte[] second)
				return CompareBinaryValues(first, second);

			return false;
		}

		#endregion

		internal static int ComputeBinaryHashCode(byte[] bytes)
		{
			var num = 0;
			var index1 = 0;
			for (var index2 = Math.Min(bytes.Length, 7); index1 < index2; ++index1)
				num = num << 5 ^ bytes[index1];

			return num;
		}

		internal static bool CompareBinaryValues(byte[] first, byte[] second)
		{
			if (first.Length != second.Length)
				return false;

			return !first.Where((t, index) => t != second[index]).Any();
		}

		#endregion
	}
}
