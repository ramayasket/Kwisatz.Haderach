using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Kw.WinAPI;

namespace Kw.WinAPI
{
	[StructLayout(LayoutKind.Sequential)]
	public struct WTS_SESSION_INFO
	{
		public int SessionID;
		[MarshalAs(UnmanagedType.LPStr)]
		public string pWinStationName;
		public WTS_CONNECTSTATE_CLASS State;
	}

	public enum WTS_INFO_CLASS
	{
		WTSInitialProgram, WTSApplicationName, WTSWorkingDirectory, WTSOEMId, WTSSessionId, WTSUserName, WTSWinStationName,
		WTSDomainName, WTSConnectState, WTSClientBuildNumber, WTSClientName, WTSClientDirectory, WTSClientProductId,
		WTSClientHardwareId, WTSClientAddress, WTSClientDisplay, WTSClientProtocolType
	}

	public enum WTS_CONNECTSTATE_CLASS
	{
		WTSActive, WTSConnected, WTSConnectQuery, WTSShadow, WTSDisconnected, WTSIdle, WTSListen, WTSReset, WTSDown, WTSInit
	}

	public struct WTS_USER_INFO
	{
		public int SessionID;
		public string UserName;
		public string Domain;
		public WTS_CONNECTSTATE_CLASS State;

		public string FullName => $"{Domain}\\{UserName}";
	}

	public static class WtsUtils
	{
		public static IntPtr OpenServer(String Name)
		{
			IntPtr server = WtsApi32.WTSOpenServer(Name); return server;
		}

		public static void CloseServer(IntPtr ServerHandle)
		{
			WtsApi32.WTSCloseServer(ServerHandle);
		}

		public static WTS_USER_INFO[] EnumerateUsers(string serverName = null)
		{
			var list = new List<WTS_USER_INFO>();

			serverName = serverName ?? Environment.MachineName;

			var serverHandle = OpenServer(serverName);

			try
			{
				var sessionInfoPtr = IntPtr.Zero;
				var sessionCount = 0;

				var retVal = WtsApi32.WTSEnumerateSessions(serverHandle, 0, 1, ref sessionInfoPtr, ref sessionCount);
				var dataSize = Marshal.SizeOf(typeof(WTS_SESSION_INFO));
				var currentSession = (long) sessionInfoPtr;

				if (retVal != 0)
				{
					for (int i = 0; i < sessionCount; i++)
					{
						var si = (WTS_SESSION_INFO) Marshal.PtrToStructure((IntPtr) currentSession, typeof(WTS_SESSION_INFO));

						currentSession += dataSize;

						IntPtr userPtr;
						IntPtr domainPtr;
						uint bytes;

						WtsApi32.WTSQuerySessionInformation(serverHandle, si.SessionID, WTS_INFO_CLASS.WTSUserName, out userPtr, out bytes);
						WtsApi32.WTSQuerySessionInformation(serverHandle, si.SessionID, WTS_INFO_CLASS.WTSDomainName, out domainPtr, out bytes);

						var userName = Marshal.PtrToStringAnsi(userPtr);
						var domainName = Marshal.PtrToStringAnsi(domainPtr);

						var info = new WTS_USER_INFO
						{
							State = si.State,
							Domain = domainName,
							UserName = userName,
							SessionID = si.SessionID
						};

						list.Add(info);
					}

					WtsApi32.WTSFreeMemory(sessionInfoPtr);
				}
			}
			finally
			{
				CloseServer(serverHandle);
			}

			return list.ToArray();
		}
	}

	public static class WtsApi32
	{
		[DllImport("wtsapi32.dll")]
		public static extern IntPtr WTSOpenServer([MarshalAs(UnmanagedType.LPStr)] String pServerName);

		[DllImport("wtsapi32.dll")]
		public static extern void WTSCloseServer(IntPtr hServer);

		[DllImport("wtsapi32.dll")]
		public static extern Int32 WTSEnumerateSessions(IntPtr hServer, [MarshalAs(UnmanagedType.U4)] Int32 Reserved, [MarshalAs(UnmanagedType.U4)] Int32 Version, ref IntPtr ppSessionInfo, [MarshalAs(UnmanagedType.U4)] ref Int32 pCount);

		[DllImport("wtsapi32.dll")]
		public static extern void WTSFreeMemory(IntPtr pMemory);

		[DllImport("Wtsapi32.dll")]
		public static extern bool WTSQuerySessionInformation(System.IntPtr hServer, int sessionId, WTS_INFO_CLASS wtsInfoClass, out System.IntPtr ppBuffer, out uint pBytesReturned);
	}
}
