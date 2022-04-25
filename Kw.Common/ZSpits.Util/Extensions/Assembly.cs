using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Kw.Common.ZSpitz.Util
{
    public static class AssemblyExtensions
    {
        public static IEnumerable<T> GetAttributes<T>(this Assembly asm, bool inherit) where T : Attribute =>
            asm.GetCustomAttributes(typeof(T), inherit).Cast<T>();
    }
}
