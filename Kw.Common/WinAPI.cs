using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Kw.Common
{
	/// <summary>
	/// PInvoke-спецификации необходимые сборке Kw.Common
	/// </summary>
	internal static class WinAPI
	{
		[DllImport("kernel32.dll")]
		public static extern int GetCurrentThreadId();
	}
}

