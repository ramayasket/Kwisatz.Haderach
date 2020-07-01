using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.ServiceProcess;
using System.Text;
using Kw.Common;
using Kw.WinAPI;

namespace Kw.ServiceProcess
{
    public sealed class ServiceProgram
    {
        public readonly ServiceRunner Runner;

        public ServiceProgram(ServiceRunner runner)
        {
            if (runner == null) throw new ArgumentNullException("runner");

            Runner = runner;
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public int ServiceMain(UnhandledExceptionEventHandler handler, params string[] parameters)
        {
            return Run(handler, parameters);
        }


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public int ServiceMain(params string[] parameters)
        {
            return Run(ReportFatalError, parameters);
        }

        private int Run(UnhandledExceptionEventHandler handler, params string[] parameters)
        {
            if(null != handler)
            {
                AppDomain.CurrentDomain.UnhandledException += handler;
            }

            AppCore.PrintingPrivate = true;
            AppCore.Initialize();

            var consoleMode = false;

            if (parameters.Any())
            {
                //    we have arguments in command line

                String sOption = parameters[0];
                sOption = sOption.ToLower().Replace("/", "-");

                switch (sOption)
                {
                    case "-install":
                        {
                            return RunInstall();
                        }

                    case "-console":
                        {
                            consoleMode = true;
                            Runner.Run(parameters);
                            break;
                        }

                    default:
                        {
                            if (!Runner.UserCommandLine(parameters))
                                return -1;

                            break;
                        }
                }
            }

            if (!consoleMode)
            {
                //
                //    Other than -console and -install, assume Windows Service startup mode.
                //
                ServiceBase.Run(Runner);
            }

            AppCore.Write($"@PX {Runner.Name} has finished with exit code {Runner.ExitCode}.");

            KernelUtils.TerminateCurrentProcess((uint)Runner.ExitCode);

            //    This is just for fun.
            return int.MaxValue;
        }

        private void ReportFatalError(object sender, UnhandledExceptionEventArgs fatal)
        {
            AppCore.WriteLine("@PX ******* FATAL ERROR  *******");
            AppCore.WriteLine("@PX {0}", ((Exception)fatal.ExceptionObject).Message);

            KernelUtils.TerminateCurrentProcess();
        }

        /// <summary>
        /// To install/uninstall the service, type: "Mod.ServiceHost.exe -Install [-u]"
        /// </summary>
        /// <returns>Win32 exit code.</returns>
        private int RunInstall()
        {
            var win32Params = Environment.GetCommandLineArgs();

            var serviceParams = new string[win32Params.Length - 1];

            //    Get the full path to the running assembly
            Assembly module = System.Reflection.Assembly.GetEntryAssembly();
            serviceParams[0] = module.Location;

            Array.Copy(win32Params, 2, serviceParams, 1, serviceParams.Length - 1);

            //    Create app domain so service installer runs isolated
            AppDomain srvInstallDomain = AppDomain.CreateDomain("ServiceInstallation");

            //    Obtain folder where .NET is installed: find mscorlib.dll which holds System.Object
            Type objectType = typeof(object);
            String mscorlibPath = objectType.Assembly.Location;

            //    Construct service installation command line
            var cmdLine = mscorlibPath.Substring(0, mscorlibPath.LastIndexOf("\\", System.StringComparison.Ordinal));
            cmdLine += "\\InstallUtil.exe";

            //    Run service installation in isolated domain
            return srvInstallDomain.ExecuteAssembly(cmdLine, serviceParams);
        }
    }
}

