using Newtonsoft.Json;
using System;
using System.IO;

namespace Kw.Common
{
    /// <summary>
    /// Library-wide helper class.
    /// </summary>
    public static class Kwisarath
    {
        private static bool _exiting;

        /// <summary>
        /// True when the application is being shutting down. Setting this flag is irreversible.
        /// </summary>
        public static bool Exiting
        {
            get => _exiting;
            set => _exiting |= value;
        }

        /// <summary>
        /// True when the application is paused.
        /// </summary>
        public static bool Paused { get; set; }

        /// <summary>
        /// True when the application is not being shutting down.
        /// </summary>
        public static bool Runnable => !Exiting;

        /// <summary>
        /// Text writer for library-wide outputs.
        /// </summary>
        public static TextWriter Out { get; set; } = Console.Out;

        /// <summary>
        /// Library-wide output routine. Writes out a formatted string and a new line, using the same semantics as System.String.Format(System.String,System.Object).
        /// </summary>
        /// <param name="format">
        /// A composite format string.
        /// </param>
        /// <param name="arg">
        /// An object array that contains zero or more objects to format and write.
        /// </param>
        public static void WriteLine(string format, params object[] arg) => Out?.WriteLine(format, arg);

        /// <summary>
        /// Library-wide output routine. Writes out a formatted string, using the same semantics as System.String.Format(System.String,System.Object).
        /// </summary>
        /// <param name="format">
        /// A composite format string.
        /// </param>
        /// <param name="arg">
        /// An object array that contains zero or more objects to format and write.
        /// </param>
        public static void Write(string format, params object[] arg) => Out?.Write(format, arg);
    }
    
    /// <summary>
    /// Library-wide typed storage.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class Kwisarath<T>
    {
        /// <summary>
        /// Library-wide static instance.
        /// </summary>
        public static T Instance { get; set; }
    }
}

