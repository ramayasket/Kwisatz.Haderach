using Kw.Common.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Kw.Common
{

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
	public class ConstitutionAttribute : Attribute { }

	public static class Constitution
	{
		private readonly static List<Action<Assembly>> _assemblyHandlers = new List<Action<Assembly>>();
		private readonly static List<Action<Type>> _typeHandlers = new List<Action<Type>>();

		public static void AddAssemblyHandler(Action<Assembly> handler)
		{
			if (handler == null) throw new ArgumentNullException(nameof(handler));

			lock (_assemblyHandlers)
			{
				_assemblyHandlers.Add(handler);
			}
		}

		public static void AddTypeHandler(Action<Type> handler)
		{
			if (handler == null) throw new ArgumentNullException(nameof(handler));

			lock (_typeHandlers)
			{
				_typeHandlers.Add(handler);
			}
		}

		// ReSharper disable once InconsistentlySynchronizedField
		internal static Action<Assembly>[] AssemblyHandlers => _assemblyHandlers.ToArray();

		// ReSharper disable once InconsistentlySynchronizedField
		internal static Action<Type>[] TypeHandlers => _typeHandlers.ToArray();
	}

	/// <summary>
	/// Обеспечивает базовую функциональность
	/// </summary>
	public static class AppCore
	{
		/// <summary>
		/// Initializes application. Calls constitution handlers.
		/// </summary>
		public static void Initialize()
		{
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				EnrollAssembly(assembly);
			}

			AppDomain.CurrentDomain.AssemblyLoad += OnDomainAssemblyLoad;
		}

		private static void OnDomainAssemblyLoad(object sender, AssemblyLoadEventArgs args)
		{
			var asm = args.LoadedAssembly;

			EnrollAssembly(asm);
		}

		private static void EnrollAssembly(Assembly assembly)
		{
			var cons = assembly.GetCustomAttributes(typeof(ConstitutionAttribute), false);

			if (cons.Any())
			{
				foreach (var action in Constitution.AssemblyHandlers)
				{
					action(assembly);
				}

				foreach (var type in assembly.GetTypes())
				{
					foreach (var action in Constitution.TypeHandlers)
					{
						action(type);
					}
				}
			}
		}

		static AppCore()
		{
			PrintingInitialized = false;
			AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

			AppDomain.CurrentDomain.ProcessExit += OnDomainUnload;

			Configure();
		}

		/// <summary>
		/// By default, print date and time with current thread culture.
		/// </summary>
		/// <returns>
		/// Prefix string.
		/// </returns>
		/// <returns>
		/// ReSharper disable SpecifyACultureInStringConversionExplicitly
		/// </returns>
		private static string DefaultPrintingPrefix()
		{
			return DateTime.Now.ToString(/*CultureInfo.InvariantCulture*/);
		}

		private static void OnDomainUnload(object sender, EventArgs eventArgs)
		{
			//StopPrinting();
		}

		private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs args)
		{
			var e = args.ExceptionObject as Exception;

			if (null != e)
			{
				ReportException(e);
			}
		}

		/// <summary>
		/// Indicates application wants to go off. Means do not begin anything, wrap off what you have and roll out.
		/// </summary>
		/// <remarks>
		/// Exiting cannot be changed from True to False. Once we began to exit, there were no way back.
		/// </remarks>
		public static bool Exiting
		{
			get { return _exiting; }
			set { if (value) { _exiting = true; } }
		}

		public static volatile bool _exiting;
		public static volatile bool Paused;

		public static event Action Stopping;

		public static bool Runnable => (!Exiting && !Paused);

		const string DEFAULT_PRINTING_FILE = "Kw.App.log";
		internal static void Configure()
		{
			Printing = AppConfig.Setting("printing", Printing.Console);	//	!migrated
			PrintingFilePath = AppConfig.Setting("printing_file_path", ".\\");	//	!migrated
		}

		private static int _killTimeout = 2000;	//	default value

		public static int KillTimeout
		{
			get { return _killTimeout; }
			set
			{
				if (value < 1)
					throw new ArgumentOutOfRangeException("value");

				_killTimeout = value;
			}
		}

		public static void Stop()
		{
			if (null != Stopping)
				Stopping();

			////DbCommandRegistry.KillThemAll();

			Exiting = true;

			var sw = Stopwatch.StartNew();

			var killed = ExecutionThread.KillActiveTasks(KillTimeout);

			sw.Stop();

			if (0 != killed)
			{
				WriteLine(@"@PX Terminated {0} running tasks in {1}.", killed, sw.Elapsed);
			}
		}

		public static void StopPrinting()
		{
			Printing = 0;
		}

		#region Printing

		public static string Apart(this string line)
		{
			return Environment.NewLine + line + Environment.NewLine;
		}

		public static bool PrintingFileAppend { get; set; }

		public static Printing Printing { get; private set; }

		static Func<string> _onPrintingPrefix = DefaultPrintingPrefix;

		public static Func<string> OnPrintingPrefix
		{
			get { return _onPrintingPrefix; }
			set
			{
				_onPrintingPrefix = value ?? DefaultPrintingPrefix;
			}
		}

		public static bool PrintingPrivate { get; set; }
		public static string PrintingFilePath { get; set; }
		public static string PrintingFileName { get; set; }

		public static bool PrintingInitialized { get; set; }

		static string Format(string format)
		{
			var printingPrefix = (null == OnPrintingPrefix) ? string.Empty : OnPrintingPrefix();
			return format.Replace("@PX", printingPrefix);
		}

		static string MakeMessage(string format, params object[] arguments)
		{
			var format2 = Format(format);

			return string.Format(format2, arguments);
		}

		static readonly object _consoleLock = new object();
		static readonly object _fileLock = new object();
		static readonly object _debugLock = new object();

		static void OutputMessage(string message, Printing printing = (Printing)0)
		{
			if (!PrintingInitialized)
			{
				InitializePrinting();
			}

			if (0 == printing)
			{
				printing = Printing;
			}

			if ((printing & Printing.Console) == Printing.Console)
			{
				lock (_consoleLock)
				{
					Console.Write(message);
				}
			}

			if ((printing & Printing.Debug) == Printing.Debug)
			{
				var flatMessage = message.Replace(Environment.NewLine, " ");

				lock (_debugLock)
				{
					DebugOutput.Prefix = OnPrintingPrefix();
					DebugOutput.WriteLine(flatMessage);
				}
			}

			if ((printing & Printing.File) == Printing.File)
			{
				lock (_fileLock)
				{
					var path = Path.Combine(PrintingFilePath ?? string.Empty, PrintingFileName);
					WriteToFile(path, true, message);
				}
			}
		}

		private static void InitializePrinting()
		{
			if (Printing.File == (Printing & Printing.File))
			{
				PrintingFileName = AppConfig.Setting("printing_filename", AppDomain.CurrentDomain.FriendlyName + ".log");	//	!migrated
				PrintingFileAppend = AppConfig.Setting("printing_file_append", false);	//	!migrated

				var badLetters = Path.GetInvalidPathChars().Concat(Path.GetInvalidFileNameChars()).Distinct().ToArray();

				if (PrintingFileName.IndexOfAny(badLetters) >= 0)
				{
					WriteLine(Printing.Console | Printing.Debug, "Invalid file name: '{0}'. Instead, '{1}' will be used for printing.", PrintingFileName, DEFAULT_PRINTING_FILE);
					PrintingFileName = DEFAULT_PRINTING_FILE;
				}

				if (!string.IsNullOrEmpty(PrintingFilePath))
				{
					Directory.CreateDirectory(PrintingFilePath);
				}

				var path = Path.Combine(PrintingFilePath ?? string.Empty, PrintingFileName);

				WriteToFile(path, PrintingFileAppend);
			}

			PrintingInitialized = true;
		}

		private static StreamWriter _logWriter;

		private static void WriteToFile(string path, bool append, string text = null)
		{
			StreamWriter writer = null;
			var opened = false;

			try
			{
				writer = _logWriter ?? new StreamWriter(path, append);

				opened = true;

				if(null != text)
				{
					writer.Write(text);
				}

				writer.Flush();
			}
			/* ReSharper disable once EmptyGeneralCatchClause */
			catch(Exception)	//	warned
			{
			}
			finally 
			{
				if(PrintingPrivate)
				{
					_logWriter = writer;
				}
				else
				{
					if(opened)
					{
						writer.Dispose();
					}
				}
			}
		}

		public static void Write(string format, params object[] arguments)
		{
			var message = MakeMessage(format, arguments);
			OutputMessage(message);
		}

		public static void WriteLine(string format, params object[] arguments)
		{
			var message = MakeMessage(format, arguments) + Environment.NewLine;
			OutputMessage(message);
		}

		public static void Write(Printing printing, string format, params object[] arguments)
		{
			var message = MakeMessage(format, arguments);
			OutputMessage(message, printing);
		}

		public static void WriteLine(Printing printing, string format, params object[] arguments)
		{
			var message = MakeMessage(format, arguments) + Environment.NewLine;
			OutputMessage(message, printing);
		}

		#endregion

		#region Exception Handling

		public static string[] GetFyCallStack()
		{
			var stackTrace = new StackTrace();
			var raw = stackTrace.ToString();

			var lines = raw.Split(Environment.NewLine).Skip(2).Where(l => l.Contains(" Kw.")).ToArray();

			return lines;
		}

		readonly static Dictionary<Exception, string> _exceptionInfo = new Dictionary<Exception, string>();

		/// <summary>
		/// Записывает информацию об исключении в лог приложения.
		/// </summary>
		/// <param name="x">Объект исключения.</param>
		/// <param name="catchers">Дполнительные обработчики.</param>
		/// <returns></returns>
		public static void ReportException(Exception x, params ExceptionCatcher[] catchers)
		{
			if (Exiting)
				return;

			var message = FormatException(x);

			WriteLine(message);

			foreach (var catcher in catchers)
			{
				var ext = x.GetType();
				var cat = catcher.ErrorType;

				if (ext == cat)
				{
					catcher.Handler(x);
				}
			}
		}

		public static string FormatException(Exception x)
		{
			//
			//	Make sure rethrow exceptions are handled.
			//

			if (RethrowException.IsValidRethrowException(x))
			{
				x = x.UnwrapRethrown();
			}

			var message = $"@PX {x.GetType().Name}: {x.GetFullMessage()}";

			string info = null;

			if (_exceptionInfo.ContainsKey(x))
			{
				info = _exceptionInfo[x];
				_exceptionInfo.Remove(x);
			}

			if (!String.IsNullOrEmpty(info))
			{
				message += $" [{info}]";
			}

			string stackInfo;

			try
			{
				stackInfo = string.Join(Environment.NewLine + " <--- ", GetRelevantStack(x));
			}
			catch	//	handled
			{
				stackInfo = "{unknown}";
			}

			message += " - " + stackInfo;

			return message;
		}

		private static string[] GetRelevantStack(Exception x)
		{
			if (null == x.StackTrace)
				return new string[0];

			var lines = x.StackTrace.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

			for(int i=0; i<lines.Length;i++)
			{
				if (lines[i].StartsWith("   at "))
				{
					lines[i] = lines[i].Substring("   at ".Length);
				}

				if (lines[i].StartsWith("   в "))
				{
					lines[i] = lines[i].Substring("   в ".Length);
				}
			}

			//var tmpLines =
			//    lines
			//        .Select(
			//            line => string.Join(" ", line.Split(" ", StringSplitOptions.RemoveEmptyEntries).Skip(1).ToArray()
			//            )
			//        )
			//        .Where(l => !l.EndsWith(")"))
			//        .ToList();

			return lines.ToArray();
		}

		public static void SetInformation(this Exception x, string info)
		{
			_exceptionInfo[x] = info;
		}

		#endregion
	}

	public static class AppCore<T> where T : class, new()
	{
		private static T _shared;

		public static T Shared
		{
			get
			{
				return _shared = _shared ?? new T();
			}
		}
	}
}

