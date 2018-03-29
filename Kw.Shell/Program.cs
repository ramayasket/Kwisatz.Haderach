using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kw.WinAPI;

namespace Kw.Shell
{
	class Program
	{
		static void Main(string[] args)
		{
			var u = Rpcrt4.UuidCreateSequential();
		}
	}
}
