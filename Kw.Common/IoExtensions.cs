using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kw.Common
{
    /// TODO XML comments
    public static class IoExtensions
    {
        static string ToFullPath(FileInfo info)
        {
            var sinfo = info.ToString();

            if ("" == info.Name && "" == info.Extension && 2 == sinfo.Length)
            {
                return sinfo + "\\";
            }

            return info.FullName;
        }

        public static bool HasReparsePoints(string path)
        {
            path = path.TrimEnd('\\');

            var info = new FileInfo(path);
            var fullname = ToFullPath(info);

            if (ReparseCheck(info.Attributes))
                return true;

            var parts = fullname.Split("\\", StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
            {
                return false;
            }

            var subpath = string.Join("\\", parts.Take(parts.Length - 1));

            return HasReparsePoints(subpath);

            //
            //    Локальная проверка
            //
            bool ReparseCheck(FileAttributes attr) => attr.HasFlag(FileAttributes.ReparsePoint);
        }
    }
}
