using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kw.Common
{
    public static class ComparableExtensions
    {
        public static bool Between<T>(this T target, T from, T to) where T : IComparable
        {
            return (target.CompareTo(from) >= 0 && target.CompareTo(to) <= 0);
        }

        public static bool Outside<T>(this T target, T from, T to) where T : IComparable
        {
            return !Between(target, from, to);
        }
    }
}

