using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using Kw.Aspects;
using Kw.Aspects.Interceptors;
using Kw.Common;
using Kw.Common.Collections;
using Kw.Common.Threading;
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
		public object Value => 1;

		//[Intercepted(typeof(NonNullCondition))]
		//[WhenNonNull("Value")]
		private string fff(string f = "zlp")
		{
			Debug.WriteLine("fff.");
			return "zlp!";
			//throw new Exception("!fff()!");
			//return "fff()";
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

		private static readonly object locker = new object();

		public static void f()
		{
			var ct = Thread.CurrentThread; // get the f() thread

			void guard(object e) { // guardian thread method

				if (!((ManualResetEvent)e).WaitOne(1000)) ct.Abort(); // kill the f() thread if overtimed
			}

			var evt = new ManualResetEvent(false); // serves as wait handle
			new Thread(guard).Start(evt); // start guardian thread

			lock (locker)
			{
				Thread.Sleep(60000);
			}

			evt.Set(); // notify the guardian thread we're done
		}

		public enum TemperatureMeasure
		{
			Kelvin,
			Celsius,
			Fahrenheit
		}

		public static double Convert<T>(double value, T from, T to) where T:Enum
		{
			return 0;
		}

		public abstract class ConvertibleMeasurement
		{
			internal abstract bool CanConvert<T>() where T: ConvertibleMeasurement;
			internal abstract double InternalConvert<T>(double value) where T : ConvertibleMeasurement;

			public double Convert<T>(double value) where T : ConvertibleMeasurement
			{
				if (CanConvert<T>())
					return InternalConvert<T>(value);

				throw new ArgumentException();
			}

		}

		public class Kelvin : ConvertibleMeasurement
		{
			internal override bool CanConvert<T>() => typeof(T).In(typeof(Celsius), typeof(Fahrenheit));

			internal override double InternalConvert<T>(double value)
			{
				var t = typeof(T);

				if (typeof(Celsius) == t) return value - 273.15;
				if (typeof(Fahrenheit) == t) return value * 1.8 - 459.67;

				throw new ArgumentException();
			}
		}

		public class Celsius : ConvertibleMeasurement
		{
			internal override bool CanConvert<T>() => typeof(T).In(typeof(Kelvin), typeof(Fahrenheit));

			internal override double InternalConvert<T>(double value)
			{
				throw new NotImplementedException();
			}
		}

		public class Fahrenheit : ConvertibleMeasurement
		{
			internal override bool CanConvert<T>() => typeof(T).In(typeof(Kelvin), typeof(Celsius));

			internal override double InternalConvert<T>(double value)
			{
				throw new NotImplementedException();
			}
		}

		class Survey
		{
			public double RealMeasuredDepth;
			public double TrueVerticalDepth;

			/// <inheritdoc />
			public override string ToString() => $"{TrueVerticalDepth} : {RealMeasuredDepth}";
		}

		private static List<Survey> Surveys = new List<Survey>();

		private static class Limits
		{
			public static double CompareTolerance = 100;
			public static double DefaultValue = 0;
		}

		/// <summary>
		/// Получить TVD (вертикальная глубина) по MD (измеренная глубина).
		/// </summary> 
		public static double GetTvd(double md)
		{
			var lastSurvey = Surveys.LastOrDefault(survey => survey.RealMeasuredDepth <= md);
			var nextSurvey = Surveys.FirstOrDefault(survey => survey.RealMeasuredDepth > md);

			if (lastSurvey != null && nextSurvey != null)
			{
				var md1 = lastSurvey.RealMeasuredDepth;
				var md2 = nextSurvey.RealMeasuredDepth;
				var tvdl = lastSurvey.TrueVerticalDepth;
				var tvd2 = nextSurvey.TrueVerticalDepth;

				if (Math.Abs(md2 - md1) < Limits.CompareTolerance)
					return tvdl;

				var k = (tvd2 - tvdl) / (md2 - md1);
				var b = tvd2 - md2 * (tvd2 - tvdl) / (md2 - md1);
				var tvd = k * md + b;

				return tvd;
			}

			return Limits.DefaultValue;
		}

		public static int Bisect<T>(IList<T> list, Func<T, bool> bisector)
		{
			int min = -1;
			int max = list.Count;

			while (max - min > 1)
			{
				int mid = (max + min) / 2;
				T midItem = list[mid];
				bool midVal = bisector(midItem);

				if (midVal)
					max = mid;
				else
					min = mid;

				Console.WriteLine($"max {max} min {min}");
			}

			return max;
		}

		public static void Main(string[] arguments)
		{
			var ring = new Ring<int>(1, 2, 3, 4, 5);

			foreach (var i in ring)
			{
				Console.WriteLine(i);
			}

			IList<string> list = new List<string>();

			for (var i = 0; i < 10; i++)
			{
				list.Add(new string((char)('A'+i), 10));
			}

			for (var i = 0; i < 10; i++)
			{
				var cc = list.Count > i ? list[i][0] : 'X';
				Func<string, bool> f = s => s.Contains(cc);

				var b = Bisect(list, f);
				Console.WriteLine($"Looking for '{cc}': result {b}");
			}



			return;

			for (double d = 1; d < 100; d += 1.0)
			{
				var s = new Survey { TrueVerticalDepth = d, RealMeasuredDepth = d * Math.PI / 2 };
				Surveys.Add(s);
			}

			var qmd = 20.0;
			var qtvd = GetTvd(qmd);

			return;

			var k = 300.0;
			var c = new Kelvin().Convert<Celsius>(k);

			return;

			using (var wc = new WebClient())
			{
				var s = wc.DownloadString("https://www.andrique.ru/favicon.png");
				var parser = new HtmlParser(s);
				while (parser.ParseNext("a", out HtmlTag tag))
					Console.WriteLine(tag.Name);
			}

			return;

			var p = new Program().fff();
			dynamic dp = p;

			DynamicMetaObject @do = dp;

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
