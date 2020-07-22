using Kw.Common;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceProcess;

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
                //    В режиме приложения ждем нажатия ENTER и останавливаем работу.
                //    В режиме службы команда на останов поступает от диспетчера служб.
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
            Qizarate.Output?.WriteLine("{0} has received external PAUSE request.", Name);

            Qizarate.Paused = true;
        }

        protected override void OnContinue()
        {
            Qizarate.Output?.WriteLine("{0} has received external CONTINUE request.", Name);

            Qizarate.Paused = false;
        }

        #endregion

        #region Реализация

        bool InternalConfigure()
        {
            Qizarate.Output?.WriteLine("Reading configuration...");

            try
            {
                Configure();
                return true;
            }
            catch(Exception x)
            {
                Qizarate.Output?.WriteLine(x.Message);
                return false;
            }
        }

        bool InternalInitialize(params string[] parameters)
        {
            Qizarate.Output?.WriteLine("{0} is initializing...", Name);

            try
            {
                Initialize(parameters);

                Qizarate.Output?.WriteLine("{0} has completed initialization.", Name);
                return true;
            }
            catch (Exception x)
            {
                Qizarate.Output?.WriteLine(x.Message);
                ExitCode = int.MinValue;
                return false;
            }
        }

        void InternalCleanup()
        {
            Qizarate.Output?.WriteLine("{0} has received external STOP request.", Name);

            Qizarate.Exiting = true;

            Qizarate.Output?.WriteLine("{0} is cleaning up...", Name);

            try
            {
                Cleanup();
            }
            catch (Exception x)
            {
                Qizarate.Output?.WriteLine(x.Message);
            }
        }

        void Introduce(bool isService)
        {
            IsService = isService;

            Qizarate.Output?.WriteLine("{0} is running in {1} mode", Assembly.GetEntryAssembly().FullName, IsService ? "service" : "console");
            //    Environment.UserDomainName

            var domain = Environment.GetEnvironmentVariable("USERDOMAIN");
            var name = Environment.GetEnvironmentVariable("USERNAME");
            var direct = Environment.CurrentDirectory;
            var host = Environment.MachineName;

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

            var user = Path.Combine(domain, name);

            Qizarate.Output?.WriteLine("{0} is running on the '{1}' box under '{2}' account.", Name, host, user);
            Qizarate.Output?.WriteLine("Current directory is '{0}'", direct);
        }

        #endregion
    }
}

