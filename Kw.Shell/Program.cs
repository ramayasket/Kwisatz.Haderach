using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kw.Common;
using Kw.WinAPI;

namespace Kw.Shell
{
	class Program
	{
		static void Main(string[] args)
		{
			var mb = ReflectionExtensions.GetCurrentMethod();
			var type = mb.DeclaringType;

			var streams = AdsUtils.ListAlternateDataStreams("C:\\zlp");
		}
	}
}
