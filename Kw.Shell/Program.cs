using System;
using System.Collections.Generic;
using System.IO;
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

			foreach (var s in streams)
			{
				var vs = AdsFile.Read(s.FilePath, s.Name);

				AdsFile.Write("Ибёнматьволк!", s.FilePath, s.Name);

				vs = AdsFile.Read(s.FilePath, s.Name);
			}
		}
	}
}
