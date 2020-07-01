using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Kw.Common;
using Kw.WinAPI;

namespace Kw.ServiceProcess
{
    /// <summary>
    /// Прокси для службы Windows (на основе <see cref="System.ServiceProcess.ServiceController"/>)
    /// </summary>
    public class LocalServiceProxy : IServiceProxy
    {
        /// <summary>
        /// Имя службы.
        /// </summary>
        public string Service { get; private set; }

        public LocalServiceProxy(string service)
        {
            Service = service;
            ServiceConnect();
        }

        /// <summary>
        /// Sets service start mode to Automatic.
        /// </summary>
        public void SetAuto()
        {
            StartMode = Win32ServiceStartMode.Automatic;
        }

        /// <summary>
        /// Sets service start mode to Manual.
        /// </summary>
        public void SetManual()
        {
            StartMode = Win32ServiceStartMode.Manual;
        }

        /// <summary>
        /// Starts the underlying service, and waits for completion.
        /// </summary>
        [DebuggerNonUserCode]
        public void Start()
        {
            ServiceConnect();

            try
            {
                _controller.Start();
                StartMode = Win32ServiceStartMode.Automatic;
            }
            catch (Exception x)
            {
                if (!CatchAcceptNativeCode(x, ERROR_SERVICE_ALREADY_RUNNING)) throw;
            }

            WaitForStatus(Win32ServiceState.Running);
        }

        /// <summary>
        /// Stop the underlying service, and waits for completion.
        /// </summary>
        [DebuggerNonUserCode]
        public void Stop()
        {
            ServiceConnect();

            try
            {
                _controller.Stop();
                //StartMode = Win32ServiceStartMode.Manual;
            }
            catch (Exception x)
            {
                if (!CatchAcceptNativeCode(x, ERROR_SERVICE_NOT_ACTIVE)) throw;
            }

            WaitForStatus(Win32ServiceState.Stopped);
        }

        /// <summary>
        /// Stops the underlying service and starts it once again.
        /// </summary>
        public void Restart()
        {
            Stop();
            Start();
        }

        /// <summary>
        /// Tells the underlying service to pause operation, and waits for completion.
        /// </summary>
        [DebuggerNonUserCode]
        public void Pause()
        {
            ServiceConnect();

            _controller.Pause();

            WaitForStatus(Win32ServiceState.Paused);
        }

        /// <summary>
        /// Tells the underlying service to resume operation after a pause, and waits for completion.
        /// </summary>
        [DebuggerNonUserCode]
        public void Continue()
        {
            ServiceConnect();

            _controller.Continue();

            WaitForStatus(Win32ServiceState.Running);
        }

        /// <summary>
        /// Waits for the underlying service to reach the specified status or for the specified time-out to expire
        /// </summary>
        /// <param name="desiredStatus">The status to wait for</param>
        [DebuggerNonUserCode]
        public void WaitForStatus(Win32ServiceState desiredStatus)
        {
            ServiceConnect();

            if (desiredStatus == 0)
                throw new ArgumentOutOfRangeException("desiredStatus");

            try
            {
                _controller.WaitForStatus((ServiceControllerStatus)desiredStatus, TimeSpan.FromSeconds(60));
            }
            catch (System.TimeoutException)
            {
            }
        }

        /// <summary>
        /// Requests current status of the underlying service
        /// </summary>
        /// <returns></returns>
        [DebuggerNonUserCode]
        public Win32ServiceInformation Interrogate()
        {
            Win32ServiceInformation information;

            ServiceConnect();

            information.State = (Win32ServiceState)_controller.Status;
            information.StartMode = StartMode;

            return information;
        }

        /// <summary>
        /// Режим запуска службы.
        /// </summary>
        public Win32ServiceStartMode StartMode
        {
            [DebuggerNonUserCode]
            get
            {
                ServiceStartMode mode = 0;

                WithServiceHandle(hService =>
                {
                    IntPtr hGlobal = IntPtr.Zero;

                    try
                    {
                        if (!Kernel.QueryServiceConfig(hService, hGlobal = Marshal.AllocHGlobal(PREALLOC_SIZE), PREALLOC_SIZE, out _unused))
                            throw new Win32Exception(Marshal.GetLastWin32Error());

                        var config = new QUERY_SERVICE_CONFIG();
                        Marshal.PtrToStructure(hGlobal, config);

                        mode = (ServiceStartMode)config.dwStartType;
                    }
                    finally
                    {
                        if (IntPtr.Zero != hGlobal)
                        {
                            Marshal.FreeHGlobal(hGlobal);
                        }
                    }
                });

                return (Win32ServiceStartMode)mode;
            }
            [DebuggerNonUserCode]
            set
            {
                if (Win32ServiceStartMode.Unknown == value)
                    throw new ArgumentException(@"Unknown isn't a valid value to set.", "value");

                WithServiceHandle(hService =>
                {
                    if (!Kernel.ChangeServiceConfig(hService, Kernel.SERVICE_NO_CHANGE, (uint)value, Kernel.SERVICE_NO_CHANGE, null, null, IntPtr.Zero, null, null, null, null))
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                });
            }
        }

        #region Реализация

        //
        //    WINERROR.H
        //
        const int ERROR_SERVICE_ALREADY_RUNNING = 1056; // An instance of the service is already running.
        const int ERROR_SERVICE_NOT_ACTIVE = 1062;      // The service has not been started.

        bool CatchAcceptNativeCode(Exception x, int code)
        {
            var win32 = x.InnerException as Win32Exception;
            return (null != win32 && win32.NativeErrorCode == code);
        }

        /// <summary>
        /// Контроллер управления службой
        /// </summary>
        ServiceController _controller;

        /// <summary>
        /// Определяет IP адрес сервера и создает ServiceController
        /// </summary>
        /// <returns>Объект ServiceController</returns>
        [DebuggerNonUserCode]
        internal ServiceController InitializeController()
        {
            return new ServiceController(Service, ".");
        }

        /// <summary>
        /// Creates a ServiceController object connected to service.
        /// </summary>
        [DebuggerNonUserCode]
        protected void ServiceConnect()
        {
            _controller = InitializeController();
        }

        #endregion

        private const int PREALLOC_SIZE = 4096;
        private uint _unused;

        /// <summary>
        /// Открывает службу на уровне Win API и передает дескриптор службы пользовательскому коду.
        /// </summary>
        /// <param name="action">Действие пользователя.</param>
        /// <param name="dwSCMAccess">Уровень доступа к SCM.</param>
        /// <param name="dwServiceAccess">Уровень доступа к службе.</param>
        [DebuggerNonUserCode]
        protected void WithServiceHandle(Action<IntPtr> action, uint dwSCMAccess = Kernel.SC_MANAGER_ALL_ACCESS, uint dwServiceAccess = Kernel.SERVICE_ALL_ACCESS)
        {
            if (action == null) throw new ArgumentNullException("action");

            IntPtr hSCManager = IntPtr.Zero, hService = IntPtr.Zero;

            try
            {
                //    Открываем SCM
                //
                hSCManager = Kernel.OpenSCManager(".", null, dwSCMAccess);

                if (IntPtr.Zero == hSCManager)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                try
                {
                    //    Открываем службу
                    //
                    hService = Kernel.OpenService(hSCManager, Service, dwServiceAccess);

                    if (IntPtr.Zero == hService)
                        throw new Win32Exception(Marshal.GetLastWin32Error());

                    //    Выполняем метод пользователя
                    //
                    action(hService);
                }
                finally
                {
                    if (IntPtr.Zero != hService)
                    {
                        Kernel.CloseServiceHandle(hService);
                    }
                }
            }
            finally
            {
                if (IntPtr.Zero != hSCManager)
                {
                    Kernel.CloseServiceHandle(hSCManager);
                }
            }
        }
    }
}
