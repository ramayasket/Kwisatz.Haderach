using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Kw.Common;
using Kw.Common.Threading;
using Kw.WinAPI;

namespace Kw.Shell
{
	partial class Program
	{
		static void f()
		{
			// ReSharper disable once EmptyEmbeddedStatement
			//while (true)
			Console.ReadLine();

			//IPAddress ipAddress = Dns.Resolve("localhost").AddressList[0];
			//TcpListener tcpl = new TcpListener(ipAddress, 49152);
			//tcpl.Start();

			//Socket newSocket = tcpl.AcceptSocket();

			Debug.WriteLine("f ended");
		}

		public static void Main(string[] arguments)
		{
			var t = ParallelTask.StartNew(f);

			var w1 = t.WaitOne(200);

			t.Thread.Interrupt();
			t.Thread.Abort();

			Kernel.TerminateThread(t.NativeThreadId, 0);
			//t.Thread



			var w2 = t.WaitOne();

			//var axes = Instance.Whiteboard;
			//var rnd = new Random();

			//for (int i = 0; i < 30; i++)
			//{
			//	Debug.WriteLine(new Random().Next(0,2));
			//}

			Console.WriteLine("Press ENTER to quit...");
			Console.ReadLine();
		}
	}
}
