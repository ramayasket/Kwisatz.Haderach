using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kw.Common;
using Kw.Common.Threading;
using Kw.WinAPI;

namespace Kw.Shell
{
	//	ReSharper disable EmptyGeneralCatchClause
	#pragma warning disable 4014

	partial class Program
	{
		static void m0()
		{
			var m_0 = FrameworkUtils.GetStackMethod(0);
			var m_1 = FrameworkUtils.GetStackMethod(1);

			m1();
		}

		static void m1()
		{
			var m_0 = FrameworkUtils.GetStackMethod(0);
			var m_1 = FrameworkUtils.GetStackMethod(1);
			var m_2 = FrameworkUtils.GetStackMethod(2);

			m2();
		}

		static void m2()
		{
		}

		public static void Main(string[] arguments)
		{
			var m_0 = FrameworkUtils.GetStackMethod(0);
			var m_1 = FrameworkUtils.GetStackMethod(1);

			m0();

			Console.WriteLine("\nPress ENTER to quit...");
			Console.ReadLine();
		}
	}
}
