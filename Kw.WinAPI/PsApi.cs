using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Kw.WinAPI
{
	public static class PsApi
	{
		[DllImport("psapi.dll")]
		public static extern uint GetProcessImageFileName(IntPtr hProcess, [Out] StringBuilder lpImageFileName, [In] [MarshalAs(UnmanagedType.U4)] int nSize);
	}
}
