using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace /* ReSharper disable once CheckNamespace */ Kw.WinAPI
{
	public enum FILE_ID_TYPE
	{
		FileIdType = 0,
		ObjectIdType = 1,
		ExtendedFileIdType = 2,
		MaximumFileIdType
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct FILE_OBJECTID_BUFFER
	{
		public struct Union
		{
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
			public byte[] BirthVolumeId;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
			public byte[] BirthObjectId;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
			public byte[] DomainId;
		}

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
		public byte[] ObjectId;

		public Union BirthInfo;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
		public byte[] ExtendedInfo;
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct FILE_ID_DESCRIPTOR
	{
		[FieldOffset(0)]
		public uint dwSize;  // Size of the struct
		[FieldOffset(4)]
		public FILE_ID_TYPE type; // Describes the type of identifier passed in. 0 == Use the FileId member of the union. 
		[FieldOffset(8)]
		public Guid guid; // A EXT_FILE_ID_128 structure containing the 128-bit file ID of the file. This is used on ReFS file systems.

		public FILE_ID_DESCRIPTOR(uint dwSize, FILE_ID_TYPE type, Guid guid) : this()
		{
			this.dwSize = dwSize;
			this.type = type;
			this.guid = guid;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct POINT
	{
		public int X;
		public int Y;

		public POINT(int x, int y)
		{
			this.X = x;
			this.Y = y;
		}

		public POINT(Point pt) : this(pt.X, pt.Y) { }

		public static implicit operator System.Drawing.Point(POINT p)
		{
			return new System.Drawing.Point(p.X, p.Y);
		}

		public static implicit operator POINT(System.Drawing.Point p)
		{
			return new POINT(p.X, p.Y);
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct MSLLHOOKSTRUCT
	{
		public POINT pt;
		public int mouseData; // be careful, this must be ints, not uints (was wrong before I changed it...). regards, cmew.
		public int flags;
		public int time;
		public UIntPtr dwExtraInfo;
	}

	[StructLayout(LayoutKind.Sequential)]
	public class KBDLLHOOKSTRUCT
	{
		public VK vkCode;
		public uint scanCode;
		public KBDLLHOOKSTRUCTFlags flags;
		public uint time;
		public UIntPtr dwExtraInfo;
	}

	[Flags]
	public enum KBDLLHOOKSTRUCTFlags : uint
	{
		LLKHF_EXTENDED = 0x01,
		LLKHF_INJECTED = 0x10,
		LLKHF_ALTDOWN = 0x20,
		LLKHF_UP = 0x80,
	}

	[StructLayout(LayoutKind.Sequential)]
	public class QUERY_SERVICE_CONFIG
	{
		[MarshalAs(UnmanagedType.U4)]
		public UInt32 dwServiceType;
		[MarshalAs(UnmanagedType.U4)]
		public UInt32 dwStartType;
		[MarshalAs(UnmanagedType.U4)]
		public UInt32 dwErrorControl;
		[MarshalAs(UnmanagedType.LPWStr)]
		public String lpBinaryPathName;
		[MarshalAs(UnmanagedType.LPWStr)]
		public String lpLoadOrderGroup;
		[MarshalAs(UnmanagedType.U4)]
		public UInt32 dwTagID;
		[MarshalAs(UnmanagedType.LPWStr)]
		public String lpDependencies;
		[MarshalAs(UnmanagedType.LPWStr)]
		public String lpServiceStartName;
		[MarshalAs(UnmanagedType.LPWStr)]
		public String lpDisplayName;
	};

	[Flags]
	public enum ProcessAccessFlags : uint
	{
		All = 0x001F0FFF,
		Terminate = 0x00000001,
		CreateThread = 0x00000002,
		VMOperation = 0x00000008,
		VMRead = 0x00000010,
		VMWrite = 0x00000020,
		DupHandle = 0x00000040,
		SetInformation = 0x00000200,
		QueryInformation = 0x00000400,
		Synchronize = 0x00100000
	}

	/* interop */
	/// ReSharper disable MemberCanBePrivate.Local
	/// ReSharper disable FieldCanBeMadeReadOnly.Local
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct BY_HANDLE_FILE_INFORMATION
	{
		public FileAttributes dwFileAttributes;
		public ulong ftCreationTime;
		public ulong ftLastAccessTime;
		public ulong ftLastWriteTime;
		public uint dwVolumeSerialNumber;
		public uint nFileSizeHigh;
		public uint nFileSizeLow;
		public uint nNumberOfLinks;
		public uint nFileIndexHigh;
		public uint nFileIndexLow;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct FILE_BASIC_INFO
	{
		public ulong CreationTime;
		public ulong LastAccessTime;
		public ulong LastWriteTime;
		public ulong ChangeTime;
		public FileAttributes FileAttributes;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct FILE_DISPOSITION_INFO
	{
		public bool DeleteFile;
	}

	/// <summary>
	/// File information by handle
	/// </summary>
	public enum FILE_INFO_BY_HANDLE_CLASS : uint
	{
		FileBasicInfo = 0,
		FileStandardInfo = 1,
		FileNameInfo = 2,
		FileRenameInfo = 3,
		FileDispositionInfo = 4,
		FileAllocationInfo = 5,
		FileEndOfFileInfo = 6,
		FileStreamInfo = 7,
		FileCompressionInfo = 8,
		FileAttributeTagInfo = 9,
		FileIdBothDirectoryInfo = 10,
		FileIdBothDirectoryRestartInfo = 11,
		FileIoPriorityHintInfo = 12,
		FileRemoteProtocolInfo = 13,
		FileFullDirectoryInfo = 14,
		FileFullDirectoryRestartInfo = 15,
		FileStorageInfo = 16,
		FileAlignmentInfo = 17,
		FileIdInfo = 18,
		FileIdExtdDirectoryInfo = 19,
		FileIdExtdDirectoryRestartInfo = 20,
		MaximumFileInfoByHandlesClass
	}
}
