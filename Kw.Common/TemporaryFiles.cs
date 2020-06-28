using System;
using System.IO;

namespace Kw.Common
{
    /// <summary>
    /// Поддиректория в папке TEMP (удаляется при Dispose)
    /// </summary>
    public class TemporaryFolder : IDisposable
    {
        private readonly string _name;
        private readonly string _temp;
        private DirectoryInfo _directory;

        public DirectoryInfo Directory
        {
            get { return _directory; }
        }

        public TemporaryFolder(string name = null)
        {
            _temp = Path.GetTempPath();
            _name = name ?? Guid.NewGuid().ToString().Replace("-", string.Empty);

            var path = Path.Combine(_temp, _name);

            _directory = new DirectoryInfo(path);

            if (_directory.Exists)
                throw new IncorrectOperationException("Folder already exists.");

            _directory.Create();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~TemporaryFolder()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Clean();
            }
        }

        private void CleanDirectory(DirectoryInfo dir)
        {
            // Get the subdirectories for the specified directory.
            var dirs = dir.GetDirectories();

            // Get the files in the directory and copy them to the new location.

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                file.Delete();
            }

            foreach (DirectoryInfo subdir in dirs)
            {
                CleanDirectory(subdir);
            }

            dir.Delete();
        }
        private void Clean()
        {
            CleanDirectory(_directory);
        }
    }

    /// <summary>
    /// Файл в папке TEMP (удаляется при Dispose).
    /// </summary>
    public class TemporaryFile : IDisposable
    {
        private readonly string _name;
        private readonly string _temp;

        /// <summary>
        /// Полный путь к файлу.
        /// </summary>
        public string Path => System.IO.Path.Combine(_temp, _name);

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="TemporaryFile"/>.
        /// </summary>
        public TemporaryFile(string format = null)
        {
            format = format ?? "{0}.temp";

            _temp = System.IO.Path.GetTempPath();
            _name = string.Format(format, Guid.NewGuid().ToString().Replace("-", string.Empty));
        }

        public static string CreateTemporaryFileName(string format = null)
        {
            format = format ?? "{0}.temp";

            var temp = System.IO.Path.GetTempPath();
            var name = string.Format(format, Guid.NewGuid().ToString().Replace("-", string.Empty));

            return System.IO.Path.Combine(temp, name);
        }

        /// <summary>
        /// Удаляет файл.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public FileStream Create()
        {
            return File.Create(Path, 0x1000);
        }

        public FileStream Create(int bufferSize)
        {
            return new FileStream(Path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, bufferSize);
        }

        public FileStream Create(int bufferSize, FileOptions options)
        {
            return new FileStream(Path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, bufferSize, options);
        }

        public StreamWriter CreateText()
        {
            return new StreamWriter(Path, false);
        }

        public FileStream Open(FileMode mode)
        {
            return File.Open(Path, mode, (mode == FileMode.Append) ? FileAccess.Write : FileAccess.ReadWrite, FileShare.None);
        }

        public FileStream Open(FileMode mode, FileAccess access)
        {
            return File.Open(Path, mode, access, FileShare.None);
        }

        public FileStream Open(FileMode mode, FileAccess access, FileShare share)
        {
            return new FileStream(Path, mode, access, share);
        }

        public FileStream OpenRead()
        {
            return new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public StreamReader OpenText()
        {
            return new StreamReader(Path);
        }

        public FileStream OpenWrite()
        {
            return new FileStream(Path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
        }

        ~TemporaryFile()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                File.Delete(Path);
            }
        }
    }
}

