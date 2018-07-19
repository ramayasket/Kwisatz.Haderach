using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Kw.WinAPI;
using Microsoft.Win32.SafeHandles;

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
		#region Utilities
		private static int MakeHRFromErrorCode(int errorCode)
		{
			return (-2147024896 | errorCode);
		}

		private static string GetErrorMessage(int errorCode)
		{
			var lpBuffer = new StringBuilder(0x200);
			if (0 != FormatMessage(0x3200, IntPtr.Zero, errorCode, 0, lpBuffer, lpBuffer.Capacity, IntPtr.Zero))
			{
				return lpBuffer.ToString();
			}

			return Resources.Error_UnknownError(errorCode);
		}

		private static void ThrowIOError(int errorCode, string path)
		{
			switch (errorCode)
			{
				case 0:
				{
					break;
				}
				case 2: // File not found
				{
					if (string.IsNullOrEmpty(path)) throw new FileNotFoundException();
					throw new FileNotFoundException(null, path);
				}
				case 3: // Directory not found
				{
					if (string.IsNullOrEmpty(path)) throw new DirectoryNotFoundException();
					throw new DirectoryNotFoundException(Resources.Error_DirectoryNotFound(path));
				}
				case 5: // Access denied
				{
					if (string.IsNullOrEmpty(path)) throw new UnauthorizedAccessException();
					throw new UnauthorizedAccessException(Resources.Error_AccessDenied_Path(path));
				}
				case 15: // Drive not found
				{
					if (string.IsNullOrEmpty(path)) throw new DriveNotFoundException();
					throw new DriveNotFoundException(Resources.Error_DriveNotFound(path));
				}
				case 32: // Sharing violation
				{
					if (string.IsNullOrEmpty(path)) throw new IOException(GetErrorMessage(errorCode), MakeHRFromErrorCode(errorCode));
					throw new IOException(Resources.Error_SharingViolation(path), MakeHRFromErrorCode(errorCode));
				}
				case 80: // File already exists
				{
					if (!string.IsNullOrEmpty(path))
					{
						throw new IOException(Resources.Error_FileAlreadyExists(path), MakeHRFromErrorCode(errorCode));
					}
					break;
				}
				case 87: // Invalid parameter
				{
					throw new IOException(GetErrorMessage(errorCode), MakeHRFromErrorCode(errorCode));
				}
				case 183: // File or directory already exists
				{
					if (!string.IsNullOrEmpty(path))
					{
						throw new IOException(Resources.Error_AlreadyExists(path), MakeHRFromErrorCode(errorCode));
					}
					break;
				}
				case 206: // Path too long
				{
					throw new PathTooLongException();
				}
				case 995: // Operation cancelled
				{
					throw new OperationCanceledException();
				}
				default:
				{
					Marshal.ThrowExceptionForHR(MakeHRFromErrorCode(errorCode));
					break;
				}
			}
		}

		public static void ThrowLastIOError(string path)
		{
			int errorCode = Marshal.GetLastWin32Error();
			if (0 != errorCode)
			{
				int hr = Marshal.GetHRForLastWin32Error();
				if (0 <= hr) throw new Win32Exception(errorCode);
				ThrowIOError(errorCode, path);
			}
		}

		public static NativeFileAccess ToNative(this FileAccess access)
		{
			NativeFileAccess result = 0;
			if (FileAccess.Read == (FileAccess.Read & access)) result |= NativeFileAccess.GenericRead;
			if (FileAccess.Write == (FileAccess.Write & access)) result |= NativeFileAccess.GenericWrite;
			return result;
		}

		public static string BuildStreamPath(string filePath, string streamName)
		{
			if (string.IsNullOrEmpty(filePath)) return string.Empty;

			// Trailing slashes on directory paths don't work:

			string result = filePath;
			int length = result.Length;
			while (0 < length && '\\' == result[length - 1])
			{
				length--;
			}

			if (length != result.Length)
			{
				result = 0 == length ? "." : result.Substring(0, length);
			}

			result += AdsUtils.StreamSeparator + streamName + AdsUtils.StreamSeparator + "$DATA";

			if (AdsUtils.MaxPath <= result.Length && !result.StartsWith(AdsUtils.LongPathPrefix))
			{
				result = AdsUtils.LongPathPrefix + result;
			}

			return result;
		}

		public static void ValidateStreamName(string streamName)
		{
			if (!string.IsNullOrEmpty(streamName) && -1 != streamName.IndexOfAny(AdsUtils.InvalidStreamNameChars))
			{
				throw new ArgumentException(Resources.Error_InvalidFileChars());
			}
		}

		private static int SafeGetFileAttributes(string name)
		{
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

			int result = GetFileAttributes(name);
			if (-1 == result)
			{
				int errorCode = Marshal.GetLastWin32Error();
				switch (errorCode)
				{
					case AdsUtils.ErrorFileNotFound:
					case AdsUtils.ErrorPathNotFound:
					{
						break;
					}
					default:
					{
						ThrowLastIOError(name);
						break;
					}
				}
			}

			return result;
		}

		public static bool FileExists(string name) => -1 != SafeGetFileAttributes(name);

		public static bool SafeDeleteFile(string name)
		{
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

			bool result = DeleteFile(name);
			if (!result)
			{
				int errorCode = Marshal.GetLastWin32Error();
				switch (errorCode)
				{
					case AdsUtils.ErrorFileNotFound:
					case AdsUtils.ErrorPathNotFound:
					{
						break;
					}
					default:
					{
						ThrowLastIOError(name);
						break;
					}
				}
			}

			return result;
		}

		public static SafeFileHandle SafeCreateFile(string path, NativeFileAccess access, FileShare share, IntPtr security, FileMode mode, NativeFileFlags flags, IntPtr template)
		{
			SafeFileHandle result = CreateFile(path, access, share, security, mode, flags, template);
			if (!result.IsInvalid && 1 != GetFileType(result))
			{
				result.Dispose();
				throw new NotSupportedException(Resources.Error_NonFile(path));
			}

			return result;
		}

		private static long GetFileSize(string path, SafeFileHandle handle)
		{
			long result = 0L;
			if (null != handle && !handle.IsInvalid)
			{
				if (GetFileSizeEx(handle, out var value))
				{
					result = value.ToInt64();
				}
				else
				{
					ThrowLastIOError(path);
				}
			}

			return result;
		}

		public static long GetFileSize(string path)
		{
			long result = 0L;
			if (!string.IsNullOrEmpty(path))
			{
				using (SafeFileHandle handle = SafeCreateFile(path, NativeFileAccess.GenericRead, FileShare.Read, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero))
				{
					result = GetFileSize(path, handle);
				}
			}

			return result;
		}

		public static IList<Win32StreamInfo> ListStreams(string filePath)
		{
			if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
			if (-1 != filePath.IndexOfAny(Path.GetInvalidPathChars())) throw new ArgumentException(Resources.Error_InvalidFileChars(), nameof(filePath));

			var result = new List<Win32StreamInfo>();

			using (SafeFileHandle hFile = SafeCreateFile(filePath, NativeFileAccess.GenericRead, FileShare.Read, IntPtr.Zero, FileMode.Open, NativeFileFlags.BackupSemantics, IntPtr.Zero))
			using (var hName = new StreamName())
			{
				if (!hFile.IsInvalid)
				{
					var streamId = new Win32StreamId();
					int dwStreamHeaderSize = Marshal.SizeOf(streamId);
					bool finished = false;
					IntPtr context = IntPtr.Zero;
					int bytesRead;

					try
					{
						while (!finished)
						{
							// Read the next stream header:
							if (!BackupRead(hFile, ref streamId, dwStreamHeaderSize, out bytesRead, false, false, ref context))
							{
								finished = true;
							}
							else if (dwStreamHeaderSize != bytesRead)
							{
								finished = true;
							}
							else
							{
								// Read the stream name:
								string name;
								if (0 >= streamId.StreamNameSize)
								{
									name = null;
								}
								else
								{
									hName.EnsureCapacity(streamId.StreamNameSize);
									if (!BackupRead(hFile, hName.MemoryBlock, streamId.StreamNameSize, out bytesRead, false, false, ref context))
									{
										name = null;
										finished = true;
									}
									else
									{
										// Unicode chars are 2 bytes:
										name = hName.ReadStreamName(bytesRead >> 1);
									}
								}

								// Add the stream info to the result:
								if (!string.IsNullOrEmpty(name))
								{
									result.Add(new Win32StreamInfo
									{
										StreamType = (FileStreamType)streamId.StreamId,
										StreamAttributes = (FileStreamAttributes)streamId.StreamAttributes,
										StreamSize = streamId.Size.ToInt64(),
										StreamName = name
									});
								}

								// Skip the contents of the stream:
								if (0 != streamId.Size.Low || 0 != streamId.Size.High)
								{
									if (!finished && !BackupSeek(hFile, streamId.Size.Low, streamId.Size.High, out _, out _, ref context))
									{
										finished = true;
									}
								}
							}
						}
					}
					finally
					{
						// Abort the backup:
						BackupRead(hFile, hName.MemoryBlock, 0, out bytesRead, true, false, ref context);
					}
				}
			}

			return result;
		}

		#endregion

		public const uint GENERIC_WRITE = 0x40000000;
		public const uint GENERIC_READ = 0x80000000;

		public const uint FILE_SHARE_READ = 0x00000001;
		public const uint FILE_SHARE_WRITE = 0x00000002;

		public const uint CREATE_NEW = 1;
		public const uint CREATE_ALWAYS = 2;
		public const uint OPEN_EXISTING = 3;
		public const uint OPEN_ALWAYS = 4;

		[DllImport("kernel32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool AllocConsole();

		[DllImport("kernel32.dll")]
		public static extern IntPtr GetConsoleWindow();

		[DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
		public static extern bool FreeConsole();


		[DllImport("kernel32.dll")]
		public static extern bool TerminateThread(IntPtr hThread, uint dwExitCode);

		[System.Runtime.InteropServices.DllImport("kernel32", SetLastError = true)]
		public static extern uint ReadFile(SafeFileHandle handle,
			byte[] buffer,
			uint byteToRead,
			ref uint bytesRead,
			IntPtr lpOverlapped);

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern bool WriteFile(SafeFileHandle handle,
			byte[] lpBuffer,
			uint nNumberOfBytesToWrite,
			ref uint lpNumberOfBytesWritten,
			IntPtr lpOverlapped);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
		public static extern int FormatMessage(
			int dwFlags,
			IntPtr lpSource,
			int dwMessageId,
			int dwLanguageId,
			StringBuilder lpBuffer,
			int nSize,
			IntPtr vaListArguments);

		[DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern int GetFileAttributes(string fileName);

		[DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetFileSizeEx(SafeFileHandle handle, out LargeInteger size);

		[System.Runtime.InteropServices.DllImport("kernel32", SetLastError = true)]
		public static extern uint GetFileSize(SafeFileHandle handle, IntPtr size);

		[DllImport("kernel32.dll")]
		private static extern int GetFileType(SafeFileHandle handle);

		[DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern SafeFileHandle CreateFile(
			string name,
			NativeFileAccess access,
			FileShare share,
			IntPtr security,
			FileMode mode,
			NativeFileFlags flags,
			IntPtr template);

		[DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool DeleteFile(string name);

		[DllImport("kernel32", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool BackupRead(
			SafeFileHandle hFile,
			ref Win32StreamId pBuffer,
			int numberOfBytesToRead,
			out int numberOfBytesRead,
			[MarshalAs(UnmanagedType.Bool)] bool abort,
			[MarshalAs(UnmanagedType.Bool)] bool processSecurity,
			ref IntPtr context);

		[DllImport("kernel32", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool BackupRead(
			SafeFileHandle hFile,
			SafeHGlobalHandle pBuffer,
			int numberOfBytesToRead,
			out int numberOfBytesRead,
			[MarshalAs(UnmanagedType.Bool)] bool abort,
			[MarshalAs(UnmanagedType.Bool)] bool processSecurity,
			ref IntPtr context);

		[DllImport("kernel32", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool BackupSeek(
			SafeFileHandle hFile,
			int bytesToSeekLow,
			int bytesToSeekHigh,
			out int bytesSeekedLow,
			out int bytesSeekedHigh,
			ref IntPtr context);

		////////



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

		public const uint GenericRead = unchecked((uint)0x80000000),
			FileFlagBackupSemantics = 0x02000000,
			OpenExisting = 3;

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern SafeFileHandle OpenFileById(IntPtr volumeHandle, ref FILE_ID_DESCRIPTOR lpFileId,
			uint dwDesiredAccess, uint dwShareMode, uint lpSecurityAttributes, uint dwFlagsAndAttributes);

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern int GetFinalPathNameByHandle(IntPtr handle, [In, Out] StringBuilder path, int bufLen, int flags);

		public const uint FsctlCreateOrGetObjectId = ((uint)EFileDevice.FileSystem << 16) | (48 << 2) | (uint)EMethod.Buffered | (0 << 14);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool DeviceIoControl(
			SafeFileHandle hDevice,
			uint dwIoControlCode,
			IntPtr lpInBuffer,
			uint nInBufferSize,
			[Out] IntPtr lpOutBuffer,
			int nOutBufferSize,
			ref uint lpBytesReturned,
			IntPtr lpOverlapped
		);

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern SafeFileHandle CreateFile(
			string fileName,
			uint dwDesiredAccess,
			FileShare dwShareMode,
			IntPtr securityAttrsMustBeZero,
			FileMode dwCreationDisposition,
			uint dwFlagsAndAttributes,
			IntPtr hTemplateFileMustBeZero
		);

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern SafeFileHandle CreateFileW(
			string fileName,
			int dwDesiredAccess,
			FileShare dwShareMode,
			IntPtr securityAttrsMustBeZero,
			FileMode dwCreationDisposition,
			int dwFlagsAndAttributes,
			IntPtr hTemplateFileMustBeZero
		);

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct FILE_ID_BOTH_DIR_INFO
		{
			public uint NextEntryOffset;
			public uint FileIndex;
			public ulong CreationTime;
			public ulong LastAccessTime;
			public ulong LastWriteTime;
			public ulong ChangeTime;
			public ulong EndOfFile;
			public ulong AllocationSize;
			public uint FileAttributes;
			public uint FileNameLength;
			public uint EaSize;
			public char ShortNameLength;
			[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 12)]
			public string ShortName;
			public ulong FileId;
			[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 1)]
			public string FileName;
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool GetFileInformationByHandleEx(IntPtr hFile, FILE_INFO_BY_HANDLE_CLASS infoClass, out FILE_ID_BOTH_DIR_INFO dirInfo, uint dwBufferSize);

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

