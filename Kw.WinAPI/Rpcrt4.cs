using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Kw.WinAPI
{
	public static class Rpcrt4
	{
		[DllImport("rpcrt4.dll", SetLastError = true)]
		public static extern int UuidCreateSequential(out Guid guid);

		public static Guid UuidCreateSequential()
		{
			UuidCreateSequential(out Guid id);
			return id;
		}
	}
}
