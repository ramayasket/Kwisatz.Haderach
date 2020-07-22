using System.Diagnostics;
using System.IO;
using System.Text;

namespace Kw.Common
{
    /// <summary>
    /// Library-wide helper class.
    /// </summary>
    public static partial class Qizarate
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
        public static TextWriter Output { get; set; } = new Writer();
    }
    
    /// <summary>
    /// Library-wide typed storage.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class Qizarate<T>
    {
        /// <summary>
        /// Library-wide static instance.
        /// </summary>
        public static T Instance { get; set; }
    }
}

