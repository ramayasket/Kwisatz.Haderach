using Kw.Common;

namespace Kw.ServiceProcess
{
    /// <summary>
    /// A managing wrapper around local service.
    /// </summary>
    public interface IServiceProxy    //    moved from Kw.Common
    {
        /// <summary>
        /// Service name.
        /// </summary>
        string Service { get; }

        /// <summary>
        /// Start mode.
        /// </summary>
        Win32ServiceStartMode StartMode { get; }

        /// <summary>
        /// Sets service start mode to Automatic.
        /// </summary>
        void SetAuto();

        /// <summary>
        /// Sets service start mode to Manual.
        /// </summary>
        void SetManual();

        /// <summary>
        /// Starts the underlying service, and waits for completion.
        /// </summary>
        void Start();

        /// <summary>
        /// Stop the underlying service, and waits for completion.
        /// </summary>
        void Stop();

        /// <summary>
        /// Stops the underlying service and starts it once again.
        /// </summary>
        void Restart();

        /// <summary>
        /// Tells the underlying service to pause operation, and waits for completion.
        /// </summary>
        void Pause();

        /// <summary>
        /// Tells the underlying service to resume operation after a pause, and waits for completion.
        /// </summary>
        void Continue();

        /// <summary>
        /// Requests current status of the underlying service
        /// </summary>
        /// <returns></returns>
        Win32ServiceInformation Interrogate();
    }
}
