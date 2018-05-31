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
		private static readonly object _lock = new object();

		private const int KILO = 1000;
		private const int MIO = KILO * KILO;

		private static int _actual;
		private static int _switched;
		private static TimeSpan _switchTime;

		private static void thread()
		{
			Interlocked.Increment(ref _actual);

			while (true)
			{
				var sw = Stopwatch.StartNew();

				lock (_lock)
				{
					sw.Stop();

					if (_switched < TEST_VOLUME)
					{
						_switched++;
						_switchTime += sw.Elapsed;
					}
					else
						return;
				}
			}
		}

		private static void Report()
		{
			Console.WriteLine($"{_switched} switches over {_actual} threads has taken {_switchTime.TotalMilliseconds} ms. (avg. {_switchTime.TotalMilliseconds/ _switched} ms. per switch)");
		}

		private const int TEST_VOLUME = 10 * MIO;
		private const int TEST_THREADS = 8;

		public static void Main(string[] arguments)
		{
			_actual = 0;
			_switched = 0;
			_switchTime = TimeSpan.Zero;

			var threads = new WaitHandle[TEST_THREADS];

			for (int i = 0; i < threads.Length; i++)
			{
				threads[i] = ExecutionThread.StartNew(thread);
			}

			threads.WaitAll();

			Report();

			_actual = 0;
			_switched = 0;
			_switchTime = TimeSpan.Zero;

			//Task.

			var tasks = new Task[TEST_THREADS];

			for (int i = 0; i < tasks.Length; i++)
			{
				tasks[i] = Task.Run((Action)thread);
			}

			Task.WaitAll(tasks);

			Report();

			Console.WriteLine("\nPress ENTER to quit...");
			Console.ReadLine();

			//Console.WriteLine("Main thread is terminating...");
		}
	}

	#pragma warning restore 4014
}
