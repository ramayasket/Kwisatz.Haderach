using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace /* ReSharper disable once CheckNamespace */ Kw.WinAPI
{
	/// <summary>
	/// Represents the attributes of a file stream.
	/// </summary>
	[Flags]
	public enum FileStreamAttributes
	{
		/// <summary>
		/// No attributes.
		/// </summary>
		None = 0,
		/// <summary>
		/// Set if the stream contains data that is modified when read.
		/// </summary>
		ModifiedWhenRead = 1,
		/// <summary>
		/// Set if the stream contains security data.
		/// </summary>
		ContainsSecurity = 2,
		/// <summary>
		/// Set if the stream contains properties.
		/// </summary>
		ContainsProperties = 4,
		/// <summary>
		/// Set if the stream is sparse.
		/// </summary>
		Sparse = 8,
	}


	/// <summary>
	/// Represents the type of data in a stream.
	/// </summary>
	public enum FileStreamType
	{
		/// <summary>
		/// Unknown stream type.
		/// </summary>
		Unknown = 0,
		/// <summary>
		/// Standard data.
		/// </summary>
		Data = 1,
		/// <summary>
		/// Extended attribute data.
		/// </summary>
		ExtendedAttributes = 2,
		/// <summary>
		/// Security data.
		/// </summary>
		SecurityData = 3,
		/// <summary>
		/// Alternate data stream.
		/// </summary>
		AlternateDataStream = 4,
		/// <summary>
		/// Hard link information.
		/// </summary>
		Link = 5,
		/// <summary>
		/// Property data.
		/// </summary>
		PropertyData = 6,
		/// <summary>
		/// Object identifiers.
		/// </summary>
		ObjectId = 7,
		/// <summary>
		/// Reparse points.
		/// </summary>
		ReparseData = 8,
		/// <summary>
		/// Sparse file.
		/// </summary>
		SparseBlock = 9,
		/// <summary>
		/// Transactional data.
		/// (Undocumented - BACKUP_TXFS_DATA)
		/// </summary>
		TransactionData = 10,
	}

	public struct Win32StreamInfo
	{
		public FileStreamType StreamType;
		public FileStreamAttributes StreamAttributes;
		public long StreamSize;
		public string StreamName;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct LargeInteger
	{
		public readonly int Low;
		public readonly int High;

		public long ToInt64()
		{
			return (High * 0x100000000) + Low;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct Win32StreamId
	{
		public readonly int StreamId;
		public readonly int StreamAttributes;
		public LargeInteger Size;
		public readonly int StreamNameSize;
	}

	[Flags]
	public enum NativeFileFlags : uint
	{
		WriteThrough = 0x80000000,
		Overlapped = 0x40000000,
		NoBuffering = 0x20000000,
		RandomAccess = 0x10000000,
		SequentialScan = 0x8000000,
		DeleteOnClose = 0x4000000,
		BackupSemantics = 0x2000000,
		PosixSemantics = 0x1000000,
		OpenReparsePoint = 0x200000,
		OpenNoRecall = 0x100000
	}

	[Flags]
	public enum NativeFileAccess : uint
	{
		GenericRead = 0x80000000,
		GenericWrite = 0x40000000
	}

	public enum FILE_ID_TYPE
	{
		FileIdType = 0,
		ObjectIdType = 1,
		ExtendedFileIdType = 2,
		MaximumFileIdType
	}

	public enum EMethod : uint
	{
		Buffered = 0,
		InDirect = 1,
		OutDirect = 2,
		Neither = 3
	}

	public enum EFileDevice : uint
	{
		Beep = 0x00000001,
		CDRom = 0x00000002,
		CDRomFileSytem = 0x00000003,
		Controller = 0x00000004,
		Datalink = 0x00000005,
		Dfs = 0x00000006,
		Disk = 0x00000007,
		DiskFileSystem = 0x00000008,
		FileSystem = 0x00000009,
		InPortPort = 0x0000000a,
		Keyboard = 0x0000000b,
		Mailslot = 0x0000000c,
		MidiIn = 0x0000000d,
		MidiOut = 0x0000000e,
		Mouse = 0x0000000f,
		MultiUncProvider = 0x00000010,
		NamedPipe = 0x00000011,
		Network = 0x00000012,
		NetworkBrowser = 0x00000013,
		NetworkFileSystem = 0x00000014,
		Null = 0x00000015,
		ParallelPort = 0x00000016,
		PhysicalNetcard = 0x00000017,
		Printer = 0x00000018,
		Scanner = 0x00000019,
		SerialMousePort = 0x0000001a,
		SerialPort = 0x0000001b,
		Screen = 0x0000001c,
		Sound = 0x0000001d,
		Streams = 0x0000001e,
		Tape = 0x0000001f,
		TapeFileSystem = 0x00000020,
		Transport = 0x00000021,
		Unknown = 0x00000022,
		Video = 0x00000023,
		VirtualDisk = 0x00000024,
		WaveIn = 0x00000025,
		WaveOut = 0x00000026,
		Port8042 = 0x00000027,
		NetworkRedirector = 0x00000028,
		Battery = 0x00000029,
		BusExtender = 0x0000002a,
		Modem = 0x0000002b,
		Vdm = 0x0000002c,
		MassStorage = 0x0000002d,
		Smb = 0x0000002e,
		Ks = 0x0000002f,
		Changer = 0x00000030,
		Smartcard = 0x00000031,
		Acpi = 0x00000032,
		Dvd = 0x00000033,
		FullscreenVideo = 0x00000034,
		DfsFileSystem = 0x00000035,
		DfsVolume = 0x00000036,
		Serenum = 0x00000037,
		Termsrv = 0x00000038,
		Ksec = 0x00000039,
		// From Windows Driver Kit 7
		Fips = 0x0000003A,
		Infiniband = 0x0000003B,
		Vmbus = 0x0000003E,
		CryptProvider = 0x0000003F,
		Wpd = 0x00000040,
		Bluetooth = 0x00000041,
		MtComposite = 0x00000042,
		MtTransport = 0x00000043,
		Biometric = 0x00000044,
		Pmi = 0x00000045
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
	public struct RECT
	{
		public int left;
		public int top;
		public int right;
		public int bottom;
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
