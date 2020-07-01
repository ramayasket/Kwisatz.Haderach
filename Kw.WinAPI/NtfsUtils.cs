using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Kw.WinAPI
{
    public static class NtfsUtils
    {
        public static string GetPathById(Guid id, char disk)
        {
            if (id == Guid.Empty)
                return null;

            StringBuilder path = new StringBuilder(512);

            var drive = $"\\\\.\\{disk}:".ToUpperInvariant();

            var hDrive = Kernel.CreateFile(drive, 0, FileShare.Read | FileShare.Write | FileShare.Delete, IntPtr.Zero, (FileMode)Kernel.OpenExisting, Kernel.FileFlagBackupSemantics, IntPtr.Zero);

            if (!hDrive.IsInvalid)
            {
                try
                {
                    FILE_ID_DESCRIPTOR desc;
                    desc.dwSize = 24;
                    desc.type = FILE_ID_TYPE.ObjectIdType;
                    desc.guid = id;

                    var h = Kernel.OpenFileById(hDrive.DangerousGetHandle(), ref desc, 0, (uint)(FileShare.Read | FileShare.Write | FileShare.Delete), 0, 0);

                    if (!h.IsInvalid)
                    {
                        Kernel.GetFinalPathNameByHandle(h.DangerousGetHandle(), path, path.Capacity, 0);
                        h.Close();

                        return path.ToString();
                    }

                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                finally
                {
                    hDrive.Close();
                }
            }

            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        public static Guid GetIdByPath(string path)
        {
            var hFile = Kernel.CreateFile(
                path,
                Kernel.GenericRead, FileShare.Read | FileShare.Write | FileShare.Delete,
                IntPtr.Zero,
                (FileMode)Kernel.OpenExisting,
                Kernel.FileFlagBackupSemantics,
                IntPtr.Zero
            );

            if (!hFile.IsInvalid)
            {
                var buffer = default(FILE_OBJECTID_BUFFER);
                var nOutBufferSize = Marshal.SizeOf(buffer);
                var lpOutBuffer = Marshal.AllocHGlobal(nOutBufferSize);
                var lpBytesReturned = default(uint);

                try
                {
                    // открываем буфер к объекту по хендлу
                    var isDeviceIoControlOpened =
                        Kernel.DeviceIoControl(
                            hFile,
                            Kernel.FsctlCreateOrGetObjectId,
                            IntPtr.Zero, 0,
                            lpOutBuffer, nOutBufferSize,
                            ref lpBytesReturned, IntPtr.Zero
                        );

                    if (isDeviceIoControlOpened)
                    {
                        buffer = (FILE_OBJECTID_BUFFER)Marshal.PtrToStructure(lpOutBuffer, typeof(FILE_OBJECTID_BUFFER));

                        Marshal.FreeHGlobal(lpOutBuffer);

                        // из буфера получаем Id объекта
                        return new Guid(buffer.ObjectId);
                    }
                    else
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                finally
                {
                    hFile.Close();
                    hFile.Dispose();
                }
            }

            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }
}
