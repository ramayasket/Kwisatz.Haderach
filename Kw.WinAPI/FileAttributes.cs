using System.IO;

namespace Kw.WinAPI
{
    /// <summary>
    /// File attributes as per WinNT.h
    /// </summary>
    public enum //    ReSharper disable once InconsistentNaming
        FILE_ATTRIBUTES
    {
        READONLY = 0x1,
        HIDDEN = 0x2,
        SYSTEM = 0x4,
        DIRECTORY = 0x10,
        ARCHIVE = 0x20,
        DEVICE = 0x40,
        NORMAL = 0x80,
        TEMPORARY = 0x100,
        SPARSE_FILE = 0x200,
        REPARSE_POINT = 0x400,
        COMPRESSED = 0x800,
        OFFLINE = 0x1000,
        NOT_CONTENT_INDEXED = 0x2000,
        ENCRYPTED = 0x4000,
        INTEGRITY_STREAM = 0x8000,
        VIRTUAL = 0x10000,
        NO_SCRUB_DATA = 0x20000,
        RECALL_ON_OPEN = 0x40000,
        RECALL_ON_DATA_ACCESS = 0x400000,
    }

    /// <summary>
    /// FILE_ATTRIBUTES as FileAttributes.
    /// </summary>
    public static class FileAttributesEx
    {
        public const FileAttributes Readonly = (FileAttributes) FILE_ATTRIBUTES.READONLY;
        public const FileAttributes Hidden = (FileAttributes) FILE_ATTRIBUTES.HIDDEN;
        public const FileAttributes System = (FileAttributes) FILE_ATTRIBUTES.SYSTEM;
        public const FileAttributes Directory = (FileAttributes) FILE_ATTRIBUTES.DIRECTORY;
        public const FileAttributes Archive = (FileAttributes) FILE_ATTRIBUTES.ARCHIVE;
        public const FileAttributes Device = (FileAttributes) FILE_ATTRIBUTES.DEVICE;
        public const FileAttributes Normal = (FileAttributes) FILE_ATTRIBUTES.NORMAL;
        public const FileAttributes Temporary = (FileAttributes) FILE_ATTRIBUTES.TEMPORARY;
        public const FileAttributes SparseFile = (FileAttributes) FILE_ATTRIBUTES.SPARSE_FILE;
        public const FileAttributes ReparsePoint = (FileAttributes) FILE_ATTRIBUTES.REPARSE_POINT;
        public const FileAttributes Compressed = (FileAttributes) FILE_ATTRIBUTES.COMPRESSED;
        public const FileAttributes Offline = (FileAttributes) FILE_ATTRIBUTES.OFFLINE;
        public const FileAttributes NotContentIndexed = (FileAttributes) FILE_ATTRIBUTES.NOT_CONTENT_INDEXED;
        public const FileAttributes Encrypted = (FileAttributes) FILE_ATTRIBUTES.ENCRYPTED;
        public const FileAttributes IntegrityStream = (FileAttributes) FILE_ATTRIBUTES.INTEGRITY_STREAM;
        public const FileAttributes Virtual = (FileAttributes) FILE_ATTRIBUTES.VIRTUAL;
        public const FileAttributes NoScrubData = (FileAttributes) FILE_ATTRIBUTES.NO_SCRUB_DATA;
        public const FileAttributes RecallOnOpen = (FileAttributes) FILE_ATTRIBUTES.RECALL_ON_OPEN;
        public const FileAttributes RecallOnDataAccess = (FileAttributes) FILE_ATTRIBUTES.RECALL_ON_DATA_ACCESS;
    }
}
