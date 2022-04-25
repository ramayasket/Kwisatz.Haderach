using System.Collections.Generic;
using System.Linq;

namespace Kw.Common.ZSpitz.Util {
    public static class TExtensions {
        public static bool Inside<T>(this T val, IEnumerable<T> vals) => vals.Contains(val);
        public static bool Inside<T>(this T val, params T[] vals) => vals.Contains(val);
        public static bool Inside(this char c, string s) => s.IndexOf(c) > -1;
        public static bool Inside<T>(this T val, HashSet<T> vals) => vals.Contains(val);

        public static bool Outside<T>(this T val, IEnumerable<T> vals) => !vals.Contains(val);
        public static bool Outside<T>(this T val, params T[] vals) => !vals.Contains(val);
        public static bool Outside(this char c, string s) => s.IndexOf(c) == -1;
        public static bool Outside<T>(this T val, HashSet<T> vals) => !vals.Contains(val);
    }
}
