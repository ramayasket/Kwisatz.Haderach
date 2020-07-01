using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Kw.WinAPI
{
    /// <summary>
    /// File-system utilities.
    /// </summary>
    public static class AdsUtils
    {
        public const int MaxPath = 256;
        internal const string LongPathPrefix = @"\\?\";
        public const char StreamSeparator = ':';
        public const int DefaultBufferSize = 0x1000;

        internal const int ErrorFileNotFound = 2;
        internal const int ErrorPathNotFound = 3;

        // "Characters whose integer representations are in the range from 1 through 31, 
        // except for alternate streams where these characters are allowed"
        // http://msdn.microsoft.com/en-us/library/aa365247(v=VS.85).aspx
        internal static readonly char[] InvalidStreamNameChars = Path.GetInvalidFileNameChars().Where(c => c < 1 || c > 31).ToArray();

        #region List Streams

        /// <summary>
        /// <span style="font-weight:bold;color:#a00;">(Extension Method)</span><br />
        /// Returns a read-only list of alternate data streams for the specified file.
        /// </summary>
        /// <param name="file">
        /// The <see cref="FileSystemInfo"/> to inspect.
        /// </param>
        /// <returns>
        /// A read-only list of <see cref="AlternateDataStreamInfo"/> objects
        /// representing the alternate data streams for the specified file, if any.
        /// If no streams are found, returns an empty list.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="file"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// The specified <paramref name="file"/> does not exist.
        /// </exception>
        /// <exception cref="SecurityException">
        /// The caller does not have the required permission. 
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The caller does not have the required permission.
        /// </exception>
        public static IList<AlternateDataStreamInfo> ListAlternateDataStreams(this FileSystemInfo file)
        {
            if (null == file) throw new ArgumentNullException(nameof(file));
            if (!file.Exists) throw new FileNotFoundException(null, file.FullName);

            string path = file.FullName;

#if NET35
            new FileIOPermission(FileIOPermissionAccess.Read, path).Demand();
#endif

            return Kernel.ListStreams(path)
                .Select(s => new AlternateDataStreamInfo(path, s))
                .ToList().AsReadOnly();
        }

        /// <summary>
        /// Returns a read-only list of alternate data streams for the specified file.
        /// </summary>
        /// <param name="filePath">
        /// The full path of the file to inspect.
        /// </param>
        /// <returns>
        /// A read-only list of <see cref="AlternateDataStreamInfo"/> objects
        /// representing the alternate data streams for the specified file, if any.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="filePath"/> is <see langword="null"/> or an empty string.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="filePath"/> is not a valid file path.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// The specified <paramref name="filePath"/> does not exist.
        /// </exception>
        /// <exception cref="SecurityException">
        /// The caller does not have the required permission. 
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The caller does not have the required permission.
        /// </exception>
        public static IList<AlternateDataStreamInfo> ListAlternateDataStreams(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (!Kernel.FileExists(filePath)) throw new FileNotFoundException(null, filePath);

#if NET35
            new FileIOPermission(FileIOPermissionAccess.Read, filePath).Demand();
#endif

            return Kernel.ListStreams(filePath)
                .Select(s => new AlternateDataStreamInfo(filePath, s))
                .ToList().AsReadOnly();
        }

        #endregion

        #region Stream Exists

        /// <summary>
        /// <span style="font-weight:bold;color:#a00;">(Extension Method)</span><br />
        /// Returns a flag indicating whether the specified alternate data stream exists.
        /// </summary>
        /// <param name="file">
        /// The <see cref="FileInfo"/> to inspect.
        /// </param>
        /// <param name="streamName">
        /// The name of the stream to find.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the specified stream exists;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="file"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="streamName"/> contains invalid characters.
        /// </exception>
        public static bool AlternateDataStreamExists(this FileSystemInfo file, string streamName)
        {
            if (null == file) throw new ArgumentNullException(nameof(file));
            Kernel.ValidateStreamName(streamName);

            string path = Kernel.BuildStreamPath(file.FullName, streamName);
            return Kernel.FileExists(path);
        }

        /// <summary>
        /// Returns a flag indicating whether the specified alternate data stream exists.
        /// </summary>
        /// <param name="filePath">
        /// The path of the file to inspect.
        /// </param>
        /// <param name="streamName">
        /// The name of the stream to find.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the specified stream exists;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="filePath"/> is <see langword="null"/> or an empty string.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para><paramref name="filePath"/> is not a valid file path.</para>
        /// <para>-or-</para>
        /// <para><paramref name="streamName"/> contains invalid characters.</para>
        /// </exception>
        public static bool AlternateDataStreamExists(string filePath, string streamName)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
            Kernel.ValidateStreamName(streamName);

            string path = Kernel.BuildStreamPath(filePath, streamName);
            return Kernel.FileExists(path);
        }

        #endregion

        #region Open Stream

        /// <summary>
        /// <span style="font-weight:bold;color:#a00;">(Extension Method)</span><br />
        /// Opens an alternate data stream.
        /// </summary>
        /// <param name="file">
        /// The <see cref="FileInfo"/> which contains the stream.
        /// </param>
        /// <param name="streamName">
        /// The name of the stream to open.
        /// </param>
        /// <param name="mode">
        /// One of the <see cref="FileMode"/> values, indicating how the stream is to be opened.
        /// </param>
        /// <returns>
        /// An <see cref="AlternateDataStreamInfo"/> representing the stream.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="file"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// The specified <paramref name="file"/> was not found.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="streamName"/> contains invalid characters.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <paramref name="mode"/> is either <see cref="FileMode.Truncate"/> or <see cref="FileMode.Append"/>.
        /// </exception>
        /// <exception cref="IOException">
        /// <para><paramref name="mode"/> is <see cref="FileMode.Open"/>, and the stream doesn't exist.</para>
        /// <para>-or-</para>
        /// <para><paramref name="mode"/> is <see cref="FileMode.CreateNew"/>, and the stream already exists.</para>
        /// </exception>
        /// <exception cref="SecurityException">
        /// The caller does not have the required permission. 
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The caller does not have the required permission, or the file is read-only.
        /// </exception>
        public static AlternateDataStreamInfo GetAlternateDataStream(this FileSystemInfo file, string streamName, FileMode mode)
        {
            if (null == file) throw new ArgumentNullException(nameof(file));
            if (!file.Exists) throw new FileNotFoundException(null, file.FullName);
            Kernel.ValidateStreamName(streamName);

            if (FileMode.Truncate == mode || FileMode.Append == mode)
            {
                throw new NotSupportedException(Resources.Error_InvalidMode(mode));
            }

#if NET35
            FileIOPermissionAccess permAccess = (FileMode.Open == mode) ? FileIOPermissionAccess.Read : FileIOPermissionAccess.Read | FileIOPermissionAccess.Write;
            new FileIOPermission(permAccess, file.FullName).Demand();
#endif

            string path = Kernel.BuildStreamPath(file.FullName, streamName);
            bool exists = Kernel.FileExists(path);

            if (!exists && FileMode.Open == mode)
            {
                throw new IOException(Resources.Error_StreamNotFound(streamName, file.Name));
            }
            if (exists && FileMode.CreateNew == mode)
            {
                throw new IOException(Resources.Error_StreamExists(streamName, file.Name));
            }

            return new AlternateDataStreamInfo(file.FullName, streamName, path, exists);
        }

        /// <summary>
        /// <span style="font-weight:bold;color:#a00;">(Extension Method)</span><br />
        /// Opens an alternate data stream.
        /// </summary>
        /// <param name="file">
        /// The <see cref="FileInfo"/> which contains the stream.
        /// </param>
        /// <param name="streamName">
        /// The name of the stream to open.
        /// </param>
        /// <returns>
        /// An <see cref="AlternateDataStreamInfo"/> representing the stream.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="file"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// The specified <paramref name="file"/> was not found.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="streamName"/> contains invalid characters.
        /// </exception>
        /// <exception cref="SecurityException">
        /// The caller does not have the required permission. 
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The caller does not have the required permission, or the file is read-only.
        /// </exception>
        public static AlternateDataStreamInfo GetAlternateDataStream(this FileSystemInfo file, string streamName)
        {
            return file.GetAlternateDataStream(streamName, FileMode.OpenOrCreate);
        }

        /// <summary>
        /// Opens an alternate data stream.
        /// </summary>
        /// <param name="filePath">
        /// The path of the file which contains the stream.
        /// </param>
        /// <param name="streamName">
        /// The name of the stream to open.
        /// </param>
        /// <param name="mode">
        /// One of the <see cref="FileMode"/> values, indicating how the stream is to be opened.
        /// </param>
        /// <returns>
        /// An <see cref="AlternateDataStreamInfo"/> representing the stream.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="filePath"/> is <see langword="null"/> or an empty string.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// The specified <paramref name="filePath"/> was not found.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para><paramref name="filePath"/> is not a valid file path.</para>
        /// <para>-or-</para>
        /// <para><paramref name="streamName"/> contains invalid characters.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <paramref name="mode"/> is either <see cref="FileMode.Truncate"/> or <see cref="FileMode.Append"/>.
        /// </exception>
        /// <exception cref="IOException">
        /// <para><paramref name="mode"/> is <see cref="FileMode.Open"/>, and the stream doesn't exist.</para>
        /// <para>-or-</para>
        /// <para><paramref name="mode"/> is <see cref="FileMode.CreateNew"/>, and the stream already exists.</para>
        /// </exception>
        /// <exception cref="SecurityException">
        /// The caller does not have the required permission. 
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The caller does not have the required permission, or the file is read-only.
        /// </exception>
        public static AlternateDataStreamInfo GetAlternateDataStream(string filePath, string streamName, FileMode mode)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (!Kernel.FileExists(filePath)) throw new FileNotFoundException(null, filePath);
            Kernel.ValidateStreamName(streamName);

            if (FileMode.Truncate == mode || FileMode.Append == mode)
            {
                throw new NotSupportedException(Resources.Error_InvalidMode(mode));
            }

#if NET35
            FileIOPermissionAccess permAccess = (FileMode.Open == mode) ? FileIOPermissionAccess.Read : FileIOPermissionAccess.Read | FileIOPermissionAccess.Write;
            new FileIOPermission(permAccess, filePath).Demand();
#endif

            string path = Kernel.BuildStreamPath(filePath, streamName);
            bool exists = Kernel.FileExists(path);

            if (!exists && FileMode.Open == mode)
            {
                throw new IOException(Resources.Error_StreamNotFound(streamName, filePath));
            }
            if (exists && FileMode.CreateNew == mode)
            {
                throw new IOException(Resources.Error_StreamExists(streamName, filePath));
            }

            return new AlternateDataStreamInfo(filePath, streamName, path, exists);
        }

        /// <summary>
        /// Opens an alternate data stream.
        /// </summary>
        /// <param name="filePath">
        /// The path of the file which contains the stream.
        /// </param>
        /// <param name="streamName">
        /// The name of the stream to open.
        /// </param>
        /// <returns>
        /// An <see cref="AlternateDataStreamInfo"/> representing the stream.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="filePath"/> is <see langword="null"/> or an empty string.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// The specified <paramref name="filePath"/> was not found.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para><paramref name="filePath"/> is not a valid file path.</para>
        /// <para>-or-</para>
        /// <para><paramref name="streamName"/> contains invalid characters.</para>
        /// </exception>
        /// <exception cref="SecurityException">
        /// The caller does not have the required permission. 
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The caller does not have the required permission, or the file is read-only.
        /// </exception>
        public static AlternateDataStreamInfo GetAlternateDataStream(string filePath, string streamName)
        {
            return GetAlternateDataStream(filePath, streamName, FileMode.OpenOrCreate);
        }

        #endregion

        #region Delete Stream

        /// <summary>
        /// <span style="font-weight:bold;color:#a00;">(Extension Method)</span><br />
        /// Deletes the specified alternate data stream if it exists.
        /// </summary>
        /// <param name="file">
        /// The <see cref="FileInfo"/> to inspect.
        /// </param>
        /// <param name="streamName">
        /// The name of the stream to delete.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the specified stream is deleted;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="file"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="streamName"/> contains invalid characters.
        /// </exception>
        /// <exception cref="SecurityException">
        /// The caller does not have the required permission. 
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The caller does not have the required permission, or the file is read-only.
        /// </exception>
        /// <exception cref="IOException">
        /// The specified file is in use. 
        /// </exception>
        public static bool DeleteAlternateDataStream(this FileSystemInfo file, string streamName)
        {
            if (null == file) throw new ArgumentNullException(nameof(file));
            Kernel.ValidateStreamName(streamName);

#if NET35
            const FileIOPermissionAccess permAccess = FileIOPermissionAccess.Write;
            new FileIOPermission(permAccess, file.FullName).Demand();
#endif

            var result = false;
            if (file.Exists)
            {
                string path = Kernel.BuildStreamPath(file.FullName, streamName);
                if (Kernel.FileExists(path))
                {
                    result = Kernel.SafeDeleteFile(path);
                }
            }

            return result;
        }

        /// <summary>
        /// Deletes the specified alternate data stream if it exists.
        /// </summary>
        /// <param name="filePath">
        /// The path of the file to inspect.
        /// </param>
        /// <param name="streamName">
        /// The name of the stream to find.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the specified stream is deleted;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="filePath"/> is <see langword="null"/> or an empty string.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para><paramref name="filePath"/> is not a valid file path.</para>
        /// <para>-or-</para>
        /// <para><paramref name="streamName"/> contains invalid characters.</para>
        /// </exception>
        /// <exception cref="SecurityException">
        /// The caller does not have the required permission. 
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The caller does not have the required permission, or the file is read-only.
        /// </exception>
        /// <exception cref="IOException">
        /// The specified file is in use. 
        /// </exception>
        public static bool DeleteAlternateDataStream(string filePath, string streamName)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
            Kernel.ValidateStreamName(streamName);

#if NET35
            const FileIOPermissionAccess permAccess = FileIOPermissionAccess.Write;
            new FileIOPermission(permAccess, filePath).Demand();
#endif

            var result = false;
            if (Kernel.FileExists(filePath))
            {
                string path = Kernel.BuildStreamPath(filePath, streamName);
                if (Kernel.FileExists(path))
                {
                    result = Kernel.SafeDeleteFile(path);
                }
            }

            return result;
        }

        #endregion
    }

    /// <summary>
    /// Represents the details of an alternative data stream.
    /// </summary>
    [DebuggerDisplay("{" + nameof(FullPath) + "}")]
    public sealed class AlternateDataStreamInfo : IEquatable<AlternateDataStreamInfo>
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AlternateDataStreamInfo"/> class.
        /// </summary>
        /// <param name="filePath">
        /// The full path of the file.
        /// This argument must not be <see langword="null"/>.
        /// </param>
        /// <param name="info">
        /// The <see cref="Win32StreamInfo"/> containing the stream information.
        /// </param>
        internal AlternateDataStreamInfo(string filePath, Win32StreamInfo info)
        {
            FilePath = filePath;
            Name = info.StreamName;
            StreamType = info.StreamType;
            Attributes = info.StreamAttributes;
            Size = info.StreamSize;
            Exists = true;

            FullPath = Kernel.BuildStreamPath(FilePath, Name);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AlternateDataStreamInfo"/> class.
        /// </summary>
        /// <param name="filePath">
        /// The full path of the file.
        /// This argument must not be <see langword="null"/>.
        /// </param>
        /// <param name="streamName">
        /// The name of the stream
        /// This argument must not be <see langword="null"/>.
        /// </param>
        /// <param name="fullPath">
        /// The full path of the stream.
        /// If this argument is <see langword="null"/>, it will be generated from the 
        /// <paramref name="filePath"/> and <paramref name="streamName"/> arguments.
        /// </param>
        /// <param name="exists">
        /// <see langword="true"/> if the stream exists;
        /// otherwise, <see langword="false"/>.
        /// </param>
        internal AlternateDataStreamInfo(string filePath, string streamName, string fullPath, bool exists)
        {
            if (string.IsNullOrEmpty(fullPath)) fullPath = Kernel.BuildStreamPath(filePath, streamName);
            StreamType = FileStreamType.AlternateDataStream;

            FilePath = filePath;
            Name = streamName;
            FullPath = fullPath;
            Exists = exists;

            if (Exists)
            {
                Size = Kernel.GetFileSize(FullPath);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns the full path of this stream.
        /// </summary>
        /// <value>
        /// The full path of this stream.
        /// </value>
        public string FullPath { get; }

        /// <summary>
        /// Returns the full path of the file which contains the stream.
        /// </summary>
        /// <value>
        /// The full file-system path of the file which contains the stream.
        /// </value>
        public string FilePath { get; }

        /// <summary>
        /// Returns the name of the stream.
        /// </summary>
        /// <value>
        /// The name of the stream.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Returns a flag indicating whether the specified stream exists.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the stream exists;
        /// otherwise, <see langword="false"/>.
        /// </value>
        public bool Exists { get; }

        /// <summary>
        /// Returns the size of the stream, in bytes.
        /// </summary>
        /// <value>
        /// The size of the stream, in bytes.
        /// </value>
        public long Size { get; }

        /// <summary>
        /// Returns the type of data.
        /// </summary>
        /// <value>
        /// One of the <see cref="FileStreamType"/> values.
        /// </value>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public FileStreamType StreamType { get; }

        /// <summary>
        /// Returns attributes of the data stream.
        /// </summary>
        /// <value>
        /// A combination of <see cref="FileStreamAttributes"/> values.
        /// </value>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public FileStreamAttributes Attributes { get; }

        #endregion

        #region Methods

        #region -IEquatable

        /// <summary>
        /// Returns a <see cref="String"/> that represents the current instance.
        /// </summary>
        /// <returns>
        /// A <see cref="String"/> that represents the current instance.
        /// </returns>
        public override string ToString()
        {
            return FullPath;
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            var comparer = StringComparer.OrdinalIgnoreCase;
            return comparer.GetHashCode(FilePath ?? string.Empty)
                   ^ comparer.GetHashCode(Name ?? string.Empty);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">
        /// An object to compare with this object.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the current object is equal to the <paramref name="obj"/> parameter;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as AlternateDataStreamInfo);
        }

        /// <summary>
        /// Returns a value indicating whether
        /// this instance is equal to another instance.
        /// </summary>
        /// <param name="other">
        /// The instance to compare to.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the current object is equal to the <paramref name="other"/> parameter;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(AlternateDataStreamInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            var comparer = StringComparer.OrdinalIgnoreCase;
            return comparer.Equals(FilePath ?? string.Empty, other.FilePath ?? string.Empty)
                   && comparer.Equals(Name ?? string.Empty, other.Name ?? string.Empty);
        }

        /// <summary>
        /// The equality operator.
        /// </summary>
        /// <param name="first">
        /// The first object.
        /// </param>
        /// <param name="second">
        /// The second object.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the two objects are equal;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator ==(AlternateDataStreamInfo first, AlternateDataStreamInfo second)
        {
            return Equals(first, second);
        }

        /// <summary>
        /// The inequality operator.
        /// </summary>
        /// <param name="first">
        /// The first object.
        /// </param>
        /// <param name="second">
        /// The second object.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the two objects are not equal;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator !=(AlternateDataStreamInfo first, AlternateDataStreamInfo second)
        {
            return !Equals(first, second);
        }

        #endregion

        #region -Delete

        /// <summary>
        /// Deletes this stream from the parent file.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the stream was deleted;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="SecurityException">
        /// The caller does not have the required permission. 
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The caller does not have the required permission, or the file is read-only.
        /// </exception>
        /// <exception cref="IOException">
        /// The specified file is in use. 
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The path of the stream is invalid.
        /// </exception>
        public bool Delete()
        {
#if NET35
            const FileIOPermissionAccess permAccess = FileIOPermissionAccess.Write;
            new FileIOPermission(permAccess, FilePath).Demand();
#endif

            return Kernel.SafeDeleteFile(FullPath);
        }

        #endregion

        #region -Open

#if NETFULL
/// <summary>
/// Calculates the access to demand.
/// </summary>
/// <param name="mode">
/// The <see cref="FileMode"/>.
/// </param>
/// <param name="access">
/// The <see cref="FileAccess"/>.
/// </param>
/// <returns>
/// The <see cref="FileIOPermissionAccess"/>.
/// </returns>
        private static FileIOPermissionAccess CalculateAccess(FileMode mode, FileAccess access)
        {
            var permAccess = FileIOPermissionAccess.NoAccess;
            switch (mode)
            {
                case FileMode.Append:
                    permAccess = FileIOPermissionAccess.Append;
                    break;

                case FileMode.Create:
                case FileMode.CreateNew:
                case FileMode.OpenOrCreate:
                case FileMode.Truncate:
                    permAccess = FileIOPermissionAccess.Write;
                    break;

                case FileMode.Open:
                    permAccess = FileIOPermissionAccess.Read;
                    break;
            }
            switch (access)
            {
                case FileAccess.ReadWrite:
                    permAccess |= FileIOPermissionAccess.Write;
                    permAccess |= FileIOPermissionAccess.Read;
                    break;

                case FileAccess.Write:
                    permAccess |= FileIOPermissionAccess.Write;
                    break;

                case FileAccess.Read:
                    permAccess |= FileIOPermissionAccess.Read;
                    break;
            }

            return permAccess;
        }
#endif

        /// <summary>
        /// Opens this alternate data stream.
        /// </summary>
        /// <param name="mode">
        /// A <see cref="FileMode"/> value that specifies whether a stream is created if one does not exist, 
        /// and determines whether the contents of existing streams are retained or overwritten.
        /// </param>
        /// <param name="access">
        /// A <see cref="FileAccess"/> value that specifies the operations that can be performed on the stream. 
        /// </param>
        /// <param name="share">
        /// A <see cref="FileShare"/> value specifying the type of access other threads have to the file. 
        /// </param>
        /// <param name="bufferSize">
        /// The size of the buffer to use.
        /// </param>
        /// <param name="useAsync">
        /// <see langword="true"/> to enable async-IO;
        /// otherwise, <see langword="false"/>.
        /// </param>
        /// <returns>
        /// A <see cref="FileStream"/> for this alternate data stream.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="bufferSize"/> is less than or equal to zero.
        /// </exception>
        /// <exception cref="SecurityException">
        /// The caller does not have the required permission. 
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The caller does not have the required permission, or the file is read-only.
        /// </exception>
        /// <exception cref="IOException">
        /// The specified file is in use. 
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The path of the stream is invalid.
        /// </exception>
        /// <exception cref="Win32Exception">
        /// There was an error opening the stream.
        /// </exception>
        public FileStream Open(FileMode mode, FileAccess access, FileShare share, int bufferSize, bool useAsync)
        {
            if (0 >= bufferSize) throw new ArgumentOutOfRangeException(nameof(bufferSize), bufferSize, null);

#if NET35
            FileIOPermissionAccess permAccess = CalculateAccess(mode, access);
            new FileIOPermission(permAccess, FilePath).Demand();
#endif

            NativeFileFlags flags = useAsync ? NativeFileFlags.Overlapped : 0;
            var handle = Kernel.SafeCreateFile(FullPath, access.ToNative(), share, IntPtr.Zero, mode, flags, IntPtr.Zero);
            if (handle.IsInvalid) Kernel.ThrowLastIOError(FullPath);
            return new FileStream(handle, access, bufferSize, useAsync);
        }

        /// <summary>
        /// Opens this alternate data stream.
        /// </summary>
        /// <param name="mode">
        /// A <see cref="FileMode"/> value that specifies whether a stream is created if one does not exist, 
        /// and determines whether the contents of existing streams are retained or overwritten.
        /// </param>
        /// <param name="access">
        /// A <see cref="FileAccess"/> value that specifies the operations that can be performed on the stream. 
        /// </param>
        /// <param name="share">
        /// A <see cref="FileShare"/> value specifying the type of access other threads have to the file. 
        /// </param>
        /// <param name="bufferSize">
        /// The size of the buffer to use.
        /// </param>
        /// <returns>
        /// A <see cref="FileStream"/> for this alternate data stream.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="bufferSize"/> is less than or equal to zero.
        /// </exception>
        /// <exception cref="SecurityException">
        /// The caller does not have the required permission. 
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The caller does not have the required permission, or the file is read-only.
        /// </exception>
        /// <exception cref="IOException">
        /// The specified file is in use. 
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The path of the stream is invalid.
        /// </exception>
        /// <exception cref="Win32Exception">
        /// There was an error opening the stream.
        /// </exception>
        public FileStream Open(FileMode mode, FileAccess access, FileShare share, int bufferSize)
        {
            return Open(mode, access, share, bufferSize, false);
        }

        /// <summary>
        /// Opens this alternate data stream.
        /// </summary>
        /// <param name="mode">
        /// A <see cref="FileMode"/> value that specifies whether a stream is created if one does not exist, 
        /// and determines whether the contents of existing streams are retained or overwritten.
        /// </param>
        /// <param name="access">
        /// A <see cref="FileAccess"/> value that specifies the operations that can be performed on the stream. 
        /// </param>
        /// <param name="share">
        /// A <see cref="FileShare"/> value specifying the type of access other threads have to the file. 
        /// </param>
        /// <returns>
        /// A <see cref="FileStream"/> for this alternate data stream.
        /// </returns>
        /// <exception cref="SecurityException">
        /// The caller does not have the required permission. 
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The caller does not have the required permission, or the file is read-only.
        /// </exception>
        /// <exception cref="IOException">
        /// The specified file is in use. 
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The path of the stream is invalid.
        /// </exception>
        /// <exception cref="Win32Exception">
        /// There was an error opening the stream.
        /// </exception>
        public FileStream Open(FileMode mode, FileAccess access, FileShare share)
        {
            return Open(mode, access, share, AdsUtils.DefaultBufferSize, false);
        }

        /// <summary>
        /// Opens this alternate data stream.
        /// </summary>
        /// <param name="mode">
        /// A <see cref="FileMode"/> value that specifies whether a stream is created if one does not exist, 
        /// and determines whether the contents of existing streams are retained or overwritten.
        /// </param>
        /// <param name="access">
        /// A <see cref="FileAccess"/> value that specifies the operations that can be performed on the stream. 
        /// </param>
        /// <returns>
        /// A <see cref="FileStream"/> for this alternate data stream.
        /// </returns>
        /// <exception cref="SecurityException">
        /// The caller does not have the required permission. 
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The caller does not have the required permission, or the file is read-only.
        /// </exception>
        /// <exception cref="IOException">
        /// The specified file is in use. 
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The path of the stream is invalid.
        /// </exception>
        /// <exception cref="Win32Exception">
        /// There was an error opening the stream.
        /// </exception>
        public FileStream Open(FileMode mode, FileAccess access)
        {
            return Open(mode, access, FileShare.None, AdsUtils.DefaultBufferSize, false);
        }

        /// <summary>
        /// Opens this alternate data stream.
        /// </summary>
        /// <param name="mode">
        /// A <see cref="FileMode"/> value that specifies whether a stream is created if one does not exist, 
        /// and determines whether the contents of existing streams are retained or overwritten.
        /// </param>
        /// <returns>
        /// A <see cref="FileStream"/> for this alternate data stream.
        /// </returns>
        /// <exception cref="SecurityException">
        /// The caller does not have the required permission. 
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The caller does not have the required permission, or the file is read-only.
        /// </exception>
        /// <exception cref="IOException">
        /// The specified file is in use. 
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The path of the stream is invalid.
        /// </exception>
        /// <exception cref="Win32Exception">
        /// There was an error opening the stream.
        /// </exception>
        public FileStream Open(FileMode mode)
        {
            FileAccess access = (FileMode.Append == mode) ? FileAccess.Write : FileAccess.ReadWrite;
            return Open(mode, access, FileShare.None, AdsUtils.DefaultBufferSize, false);
        }

        #endregion

        #region -OpenRead / OpenWrite / OpenText

        /// <summary>
        /// Opens this stream for reading.
        /// </summary>
        /// <returns>
        /// A read-only <see cref="FileStream"/> for this stream.
        /// </returns>
        /// <exception cref="SecurityException">
        /// The caller does not have the required permission. 
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The caller does not have the required permission, or the file is read-only.
        /// </exception>
        /// <exception cref="IOException">
        /// The specified file is in use. 
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The path of the stream is invalid.
        /// </exception>
        /// <exception cref="Win32Exception">
        /// There was an error opening the stream.
        /// </exception>
        public FileStream OpenRead()
        {
            return Open(FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        /// <summary>
        /// Opens this stream for writing.
        /// </summary>
        /// <returns>
        /// A write-only <see cref="FileStream"/> for this stream.
        /// </returns>
        /// <exception cref="SecurityException">
        /// The caller does not have the required permission. 
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The caller does not have the required permission, or the file is read-only.
        /// </exception>
        /// <exception cref="IOException">
        /// The specified file is in use. 
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The path of the stream is invalid.
        /// </exception>
        /// <exception cref="Win32Exception">
        /// There was an error opening the stream.
        /// </exception>
        public FileStream OpenWrite()
        {
            return Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
        }

        /// <summary>
        /// Opens this stream as a text file.
        /// </summary>
        /// <returns>
        /// A <see cref="StreamReader"/> which can be used to read the contents of this stream.
        /// </returns>
        /// <exception cref="SecurityException">
        /// The caller does not have the required permission. 
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The caller does not have the required permission, or the file is read-only.
        /// </exception>
        /// <exception cref="IOException">
        /// The specified file is in use. 
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The path of the stream is invalid.
        /// </exception>
        /// <exception cref="Win32Exception">
        /// There was an error opening the stream.
        /// </exception>
        public StreamReader OpenText()
        {
            Stream fileStream = Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            return new StreamReader(fileStream);
        }

        #endregion

        #endregion
    }
}
