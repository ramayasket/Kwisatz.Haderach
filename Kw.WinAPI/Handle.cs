using System;

namespace Kw.WinAPI
{
    public class Handle : IDisposable
    {
        private IntPtr _handle; //= IntPtr.Zero; assumed

        private bool _disposed;

        private void Dispose(bool disposing)
        {
            if (_disposed) return;        //    не освобождать повторно

            if (disposing)                //    управляемые ресурсы
            {
                Kernel.CloseHandle(_handle);
            }

            _disposed = true;
        }

        ~Handle()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public Handle(IntPtr handle)
        {
            _handle = handle;
        }

        public Handle(int handle)
        {
            _handle = new IntPtr(handle);
        }

        public static implicit operator IntPtr(Handle that)
        {
            return that._handle;
        }

        public static implicit operator int(Handle that)
        {
            return that._handle.ToInt32();
        }
    }
}

