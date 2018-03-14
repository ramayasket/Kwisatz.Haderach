using System;

namespace Kw.ServiceProcess
{
	/// <summary>
	/// Combines two service-related enums.
	/// </summary>
	[Serializable]
	public struct Win32ServiceInformation
	{
		public Win32ServiceState State;
		public Win32ServiceStartMode StartMode;
	}

	/// <summary>
	/// Enumerates Win32 service states.
	/// </summary>
	/// <remarks>
	/// This is a replica of System.ServiceProcess.ServiceControllerStatus plus 'Unknown'.
	/// </remarks>
	public enum Win32ServiceState
	{
		Unknown = 0,

		Stopped = 1,
		StartPending = 2,
		StopPending = 3,
		Running = 4,
		ContinuePending = 5,
		PausePending = 6,
		Paused = 7,
	}

	/// <summary>
	/// Enumerates Win32 service start modes.
	/// </summary>
	/// <remarks>
	/// This is a replica of System.ServiceProcess.ServiceStartMode plus 'Unknown'.
	/// </remarks>
	public enum Win32ServiceStartMode
	{
		Unknown = 0,

		Automatic = 2,
		Manual = 3,
		Disabled = 4,
	}
}

