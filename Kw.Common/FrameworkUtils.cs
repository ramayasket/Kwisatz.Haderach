using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Kw.Common
{
	/// <summary>
	/// Framework utilities
	/// </summary>
	public static class FrameworkUtils
	{
		/// <summary>
		/// Returns information about the n-th (from current) method in the call stack.
		/// </summary>
		/// <param name="offset">Offset from current method.</param>
		/// <returns>MethodBase or null if stack is less than offset.</returns>
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static MethodBase GetStackMethod(int offset = 0)
		{
			if (0 > offset)
				throw new ArgumentOutOfRangeException(nameof(offset), "Expected integer not less than 1.");

			StackTrace st = new StackTrace();
			StackFrame sf = st.GetFrame(offset + 1);

			var m = sf?.GetMethod();

			return m;
		}
	}
}
