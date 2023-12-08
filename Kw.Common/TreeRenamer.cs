using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Kw.Common
{
    /// <summary>
    /// Recursively renames directories and files.
    /// </summary>
    public static class TreeRenamer
    {
        public static int Rename(string directory, Func<string, string> renameFunction)
        {
            if(!Directory.Exists(directory ?? throw new ArgumentNullException(nameof(directory))))
                throw new ArgumentException($"Directory '{directory}' does not exist.");

            if(null == renameFunction)
                throw new ArgumentNullException(nameof(renameFunction));

            var renames = Rename(Directory.EnumerateFiles(directory).ToArray(), renameFunction);

            var directories = Directory.EnumerateDirectories(directory).ToArray();

            foreach (var d in directories)
            {
                renames += Rename(d, renameFunction);
                renames += Rename(new[] { d }, renameFunction);
            }

            return renames;
        }

        static int Rename(string[] names, Func<string, string> renameFunction)
        {
            var renames = 0;

            foreach (var name in names)
            {
                var d = Path.GetDirectoryName(name);
                var n = Path.GetFileName(name);

                var n1 = renameFunction(n);

                if (n1 != n)
                {
                    if(Directory.Exists(name))
                        Directory.Move(name, Path.Combine(d ?? string.Empty, n1));
                    else
                        File.Move(name, Path.Combine(d ?? string.Empty, n1));

                    renames++;
                }
            }

            return renames;
        }
    }
}
