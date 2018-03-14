using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Kw.WinAPI
{
	public static class Advapi32
	{
		[DllImport("advapi32.dll", SetLastError = true)]
		public static extern bool SetFileSecurityW([MarshalAs(UnmanagedType.LPWStr)] string FileName, Int32 SecurityInformation, Byte[] SecurityDescriptor);

		[DllImport("advapi32.dll", SetLastError = true)]
		public static extern bool SetKernelObjectSecurity(IntPtr Handle, Int32 SecurityInformation, Byte[] SecurityDescriptor);
	}
}
