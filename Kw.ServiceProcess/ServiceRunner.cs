using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using Kw.Common;

namespace Kw.ServiceProcess
{
	/// <summary>
	/// Точка входа двойного назначения. Инициализация/останов процесса как службы или приложения.
	/// </summary>
	public abstract class ServiceRunner : ServiceBase
	{
		/// <summary>
		/// Название службы
		/// </summary>
		public abstract string Name { get; }

		protected abstract void Configure();

		protected abstract void Initialize(params string[] parameters);

		protected abstract void Cleanup();

		public virtual bool UserCommandLine(params string[] parameters)
		{
			return false;
		}

		/// <summary>
		/// Режим запуска: 1-служба, 0-консольное приложение
		/// </summary>
		public static bool IsService { get; private set; }

		/// <summary>
		/// Работа в режиме приложения.
		/// </summary>
		public int Run(params string[] parameters)
		{
			var asm = Assembly.GetEntryAssembly();
			var loc = asm.Location;

			var path = Path.GetDirectoryName(loc);

			Debug.Assert(null != path);

			Environment.CurrentDirectory = path;

			Introduce(false);

			if (InternalConfigure() && InternalInitialize(parameters))
			{
				//
				//	В режиме приложения ждем нажатия ENTER и останавливаем работу.
				//	В режиме службы команда на останов поступает от диспетчера служб.
				//
				Console.WriteLine("Press ENTER to stop.".Apart());
				Console.ReadLine();

				InternalCleanup();
			}

			return ExitCode;
		}

		public static DirectoryInfo CreateSubdirectory(string name)
		{
			var target = Path.Combine(Environment.CurrentDirectory, name);
			var info = new DirectoryInfo(target);

			if (!info.Exists)
			{
				info.Create();
			}

			return info;
		}

	#region ServiceBase overrides

		protected sealed override void OnStart(string[] args)
		{
			var asm = Assembly.GetEntryAssembly();
			var loc = asm.Location;

			var path = Path.GetDirectoryName(loc);

			Debug.Assert(null != path);

			Environment.CurrentDirectory = path;

			Introduce(true);
			
			if (InternalConfigure())
			{
				if (!InternalInitialize(args))
				{
					Stop();
				}
			}
			else
			{
				Stop();
			}
		}

		protected sealed override void OnStop()
		{
			InternalCleanup();
		}

		protected override void OnPause()
		{
			AppCore.WriteLine("@PX {0} has received external PAUSE request.", Name);

			AppCore.Paused = true;
		}

		protected override void OnContinue()
		{
			AppCore.WriteLine("@PX {0} has received external CONTINUE request.", Name);

			AppCore.Paused = false;
		}

		#endregion

		#region Реализация

		bool InternalConfigure()
		{
			AppCore.WriteLine("@PX Reading configuration...");

			try
			{
				Configure();
				return true;
			}
			catch(Exception x)
			{
				AppCore.ReportException(x);
				return false;
			}
		}

		bool InternalInitialize(params string[] parameters)
		{
			AppCore.WriteLine("@PX {0} is initializing...", Name);

			try
			{
				Initialize(parameters);

				AppCore.WriteLine("@PX {0} has completed initialization.", Name);
				return true;
			}
			catch (Exception x)
			{
				AppCore.ReportException(x);
				ExitCode = int.MinValue;
				return false;
			}
		}

		void InternalCleanup()
		{
			AppCore.WriteLine("@PX {0} has received external STOP request.", Name);

			AppCore.Exiting = true;

			AppCore.WriteLine("@PX {0} is cleaning up...", Name);

			try
			{
				Cleanup();
			}
			catch (Exception x)
			{
				AppCore.ReportException(x);
			}
		}

		void Introduce(bool isService)
		{
			IsService = isService;

			AppCore.WriteLine("@PX {0} is running in {1} mode", Assembly.GetEntryAssembly().FullName, IsService ? "service" : "console");
			//	Environment.UserDomainName

			var domain = Environment.GetEnvironmentVariable("USERDOMAIN");
			var name = Environment.GetEnvironmentVariable("USERNAME");
			var direct = Environment.CurrentDirectory;
			var host = Environment.MachineName;
			var lpath = AppCore.PrintingFilePath;
			var lname = AppCore.PrintingFileName;

			if (string.IsNullOrEmpty(host))
			{
				host = "*LOCALHOST";
			}

			if (string.IsNullOrEmpty(domain))
			{
				domain = host;
			}

			if (string.IsNullOrEmpty(name))
			{
				name = "*CURRENT_USER";
			}

			if (string.IsNullOrEmpty(lpath))
			{
				lpath = ".";
			}

			if (string.IsNullOrEmpty(lname))
			{
				lname = "*?";
			}

			var user = Path.Combine(domain, name);
			var log = Path.Combine(lpath, lname);

			AppCore.WriteLine("@PX {0} is running on the '{1}' box under '{2}' account.", Name, host, user);
			AppCore.WriteLine("@PX Current directory is '{0}'", direct);

			if ((AppCore.Printing & Printing.File) == Printing.File)
			{
				AppCore.WriteLine("@PX Log file path is '{0}'", log);
			}
		}

		#endregion
	}
}

