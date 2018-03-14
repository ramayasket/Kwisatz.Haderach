using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Kw.WinAPI;

namespace Kw.WinAPI
{
	public static class KernelUtils
	{
		public static void TerminateCurrentProcess(uint exitCode = 0)
		{
			Kernel.TerminateProcess(Kernel.OpenProcess(ProcessAccessFlags.All, false, Kernel.GetCurrentProcessId()), exitCode);
		}

		public static void TerminateProcess(int processId, uint exitCode = 0)
		{
			using (var handle = new Handle(Kernel.OpenProcess(ProcessAccessFlags.Terminate, false, processId)))
			{
				if (IntPtr.Zero != handle)
				{
					Kernel.TerminateProcess(handle, exitCode);
				}
				//
				//	else: must have exited
			}
		}
	}

	/// <summary>
	/// Imports for kernel32.dll, advapi32.dll.
	/// </summary>
	public static class Kernel
	{
		[DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
		public static extern bool CreateHardLink(
			string lpFileName,
			string lpExistingFileName,
			IntPtr lpSecurityAttributes
		);

		[DllImport("kernel32.dll")]
		public static extern uint GetLastError();

		[DllImport("kernel32.dll")]
		public static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

		[DllImport("kernel32.dll")]
		public static extern int GetCurrentThreadId();

		[DllImport("kernel32.dll")]
		public static extern int GetCurrentProcessId();

		[DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool CloseHandle(IntPtr hObject);

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern Boolean QueryServiceConfig(IntPtr hService, IntPtr intPtrQueryConfig, UInt32 cbBufSize, out UInt32 pcbBytesNeeded);

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern Boolean ChangeServiceConfig(
			IntPtr hService,
			UInt32 nServiceType,
			UInt32 nStartType,
			UInt32 nErrorControl,
			String lpBinaryPathName,
			String lpLoadOrderGroup,
			IntPtr lpdwTagId,
			[In] char[] lpDependencies,
			String lpServiceStartName,
			String lpPassword,
			String lpDisplayName);

		[DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern IntPtr OpenService(IntPtr hSCManager, string lpServiceName, uint dwDesiredAccess);

		[DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern IntPtr OpenSCManager(string machineName, string databaseName, uint dwAccess);

		[DllImport("advapi32.dll", EntryPoint = "CloseServiceHandle")]
		public static extern int CloseServiceHandle(IntPtr hSCObject);

		//
		//	Access Rights and Constants
		//
		public const uint SERVICE_QUERY_CONFIG = 0x00000001;
		public const uint SERVICE_CHANGE_CONFIG = 0x00000002;

		public const uint SERVICE_NO_CHANGE = 0xFFFFFFFF;
		//
		public const uint SERVICE_ALL_ACCESS = 0xF01FF;
		public const uint SC_MANAGER_ALL_ACCESS = 0xF003F;

		//	ReSharper disable InconsistentNaming
		//

		[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool LookupAccountName(string lpSystemName, string lpAccountName, [MarshalAs(UnmanagedType.LPArray)] byte[] Sid, ref uint cbSid, StringBuilder ReferencedDomainName, ref uint cchReferencedDomainName, out uint peUse);

		[DllImport("advapi32", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool ConvertSidToStringSid([MarshalAs(UnmanagedType.LPArray)] byte[] pSID, out IntPtr ptrSid);

		[DllImport("kernel32.dll")]
		public static extern IntPtr LocalFree(IntPtr hMem);

		/// <summary>
		/// Функция API: вывод отладочной информации
		/// </summary>
		/// <param name="outputString">Строка для вывода</param>
		[DllImport("kernel32.dll", EntryPoint = "OutputDebugString")]
		public static extern void Output(string outputString);


		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool GetFileInformationByHandle(IntPtr hFile, out BY_HANDLE_FILE_INFORMATION lpFileInformation);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool SetFileInformationByHandle(IntPtr hFile, FILE_INFO_BY_HANDLE_CLASS FileInformationClass, ref FILE_BASIC_INFO lpFileInformation, uint dwBufferSize);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool SetFileInformationByHandle(IntPtr hFile, FILE_INFO_BY_HANDLE_CLASS FileInformationClass, ref FILE_DISPOSITION_INFO lpFileInformation, uint dwBufferSize);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool MoveFileExW([MarshalAs(UnmanagedType.LPWStr)] string lpExistingFileName, [MarshalAs(UnmanagedType.LPWStr)] string lpNewFileName, uint dwFlags);
	}
}

