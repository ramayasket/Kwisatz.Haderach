using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kw.WinAPI
{
    public class AdsFile
    {
        private AdsFile() { }

        /// <summary>
        /// Method called when an alternate data stream must be read from.
        /// </summary>
        /// <param name="file">The fully qualified name of the file from which
        /// the ADS data will be read.</param>
        /// <param name="stream">The name of the stream within the "normal" file
        /// from which to read.</param>
        /// <returns>The contents of the file as a string.  It will always return
        /// at least a zero-length string, even if the file does not exist.</returns>
        public static string Read (string file, string stream)
        {
            var fHandle = Kernel.CreateFile(file + ":" + stream,   // Filename
                Kernel.GENERIC_READ,          // Desired access
                FileShare.Read,       // Share more
                IntPtr.Zero,           // Attributes
                FileMode.Open,         // Creation attributes
                0,                     // Flags and attributes
                IntPtr.Zero);          // Template file

            // if the handle returned is uint.MaxValue, the stream doesn't exist.
            if (!fHandle.IsInvalid)
            {
                // A handle to the stream within the file was created successfully.
                uint size = Kernel.GetFileSize(fHandle, IntPtr.Zero);
                byte[] buffer = new byte[size];
                uint read = uint.MinValue;

                uint result = Kernel.ReadFile(fHandle,         // Handle
                    buffer,          // Data buffer
                    size,            // Bytes to read
                    ref read,            // Bytes actually read
                    IntPtr.Zero);    // Overlapped

                fHandle.Close();

                // Convert the bytes read into an UTF8 string and return it to the caller.
                return System.Text.Encoding.UTF8.GetString(buffer);
            }
            else
                throw new StreamNotFoundException(file, stream);
        }

        /// <summary>
        /// The static method to call when data must be written to a stream.
        /// </summary>
        /// <param name="data">The string data to embed in the stream in the file</param>
        /// <param name="file">The fully qualified name of the file with the
        /// stream into which the data will be written.</param>
        /// <param name="stream">The name of the stream within the normal file to
        /// write the data.</param>
        /// <returns>An unsigned integer of how many bytes were actually written.</returns>
        public static uint Write(string data, string file, string stream)
        {
            // Convert the string data to be written to an array of ascii characters.
            byte[] barData = Encoding.UTF8.GetBytes(data);
            uint nReturn = 0;

            var fHandle = Kernel.CreateFile(file + ":" + stream,        // File name
                Kernel.GENERIC_WRITE,              // Desired access
                FileShare.ReadWrite,           // Share mode
                IntPtr.Zero,                // Attributes
                FileMode.Truncate,              // Creation disposition
                0,                          // Flags and attributes
                IntPtr.Zero);               // Template file

            bool bOK = Kernel.WriteFile(fHandle,                        // Handle
                barData,                        // Data buffer
                (uint)barData.Length,                 // Buffer size
                ref nReturn,                        // Bytes written
                IntPtr.Zero);                   // Overlapped

            fHandle.Close();

            // Throw an exception if the data wasn't written successfully.
            if (!bOK)
                throw new System.ComponentModel.Win32Exception(System.Runtime.InteropServices.Marshal.GetLastWin32Error());

            return nReturn;
        }
    }

    /// <summary>
    /// Class to allow stream read operations to raise specific errors if the stream
    /// is not found in the file.
    /// </summary>
    public class StreamNotFoundException : System.IO.FileNotFoundException
    {
        #region Private Members
        private string _stream = string.Empty;
        #endregion

        #region ctors
        /// <summary>
        /// Constructor called with the name of the file and stream which was
        /// unsuccessfully opened.
        /// </summary>
        /// <param name="file">Fully qualified name of the file in which the stream
        /// was supposed to reside.</param>
        /// <param name="stream">Stream within the file to open.</param>
        public StreamNotFoundException
        (string file,
            string stream) : base(string.Empty, file)
        {
            _stream = stream;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Read-only property to allow the user to query the exception to determine
        /// the name of the stream that couldn't be found.
        /// </summary>
        public string Stream
        {
            get
            {
                return _stream;
            }
        }
        #endregion

        #region Overridden Properties
        /// <summary>
        /// Overridden Message property to return a concise string describing the
        /// exception.
        /// </summary>
        public override string Message
        {
            get
            {
                return "Stream \"" + _stream + "\" not found in \"" + base.FileName + "\"";
            }
        }
        #endregion
    }
}
