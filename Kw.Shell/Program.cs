using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Kw.Aspects;
using Kw.Aspects.Interceptors;
using Kw.Common;
using Kw.WinAPI;
using PostSharp.Aspects;

namespace Kw.Shell
{
	//	ReSharper disable EmptyGeneralCatchClause
#pragma warning disable 4014
#pragma warning disable 0162

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

			m2(null);
		}

		[NonNullability()]
		static void m2(object zlp)
		{
		}

		class Def
		{
			public string name;
			public string code;
		}

		[SynchronizedProperty]
		public object Value => null;

		[Intercepted(typeof(Protection),typeof(NonNullability),typeof(Synchronization)),NonNullable("f")]
		private object fff(string f = "zlp")
		{
			Debug.WriteLine("fff.");
			throw new Exception("!fff()!");
			return "fff()";
		}

		class MyInterceptor : Interceptor
		{
			/// <inheritdoc />
			public MyInterceptor(Interceptor next) : base(next) { }

			/// <inheritdoc />
			public override void Invoke(MethodInterceptionArgs args)
			{
				Debug.WriteLine("Entering interception.");
				Next.Invoke(args);
				Debug.WriteLine("Exiting interception.");
			}
		}

		public static void Main(string[] arguments)
		{
			var f0 = new Program().fff();
			Debug.WriteLine("Intercepted function.");

			return;

			m2(null);

			var v = new Program().Value;

			//const string LINE = "#define DBG_COMMAND_EXCEPTION            ((NTSTATUS)0x40010009L)    // winnt";

			var rx = new Regex(@"#define\s+(?<name>\S+)\s+\(\(NTSTATUS\)(?<code>.+)\).*", RegexOptions.Compiled);

			var lines = File.ReadAllLines("C:\\!i\\nts.h");
			var map = new HashSet<Def>();

			foreach (var line in lines)
			{
				var match = rx.Match(line);

				if (rx.IsMatch(line))
				{
					var m = rx.Match(line);

					var def = new Def {  code = m.Groups["code"].Value, name = m.Groups["name"].Value };
					map.Add(def);
				}
			}

			var defs = map.OrderBy(d => d.code).ToList();
			var maxName = defs.Select(d => d.name).Max(n => n.Length);

			Console.WriteLine("	public enum NTSTATUS : uint");
			Console.WriteLine("	{");

			foreach (var def in defs)
			{
				var name = def.name.PadRight(maxName);
				Console.WriteLine($"		{name} = {def.code.ToLower().Replace("l", "")},");
			}

			Console.WriteLine("	}");

			//Console.WriteLine("\nPress ENTER to quit...");
			//Console.ReadLine();
		}
	}
}
