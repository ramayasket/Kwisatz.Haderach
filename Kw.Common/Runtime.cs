using System;

namespace Kw.Common
{
    /// <summary>
    /// </summary>
    public static class Runtime
    {
        public static long Collect()
        {
            return GC.GetTotalMemory(true);
        }
    
        /// <summary>
        /// Nullifies reference in the way which isn't optimized away.
        /// </summary>
        /// <param name="o"></param>
        public static void Release<T>(ref T o) where T:class
        {
            if (!Equals(o, GetNull<T>()))
            {
                SetNull(ref o);
            }
        }

        static T GetNull<T>() where T : class
        {
            return (T)null;
        }

        //    ReSharper disable once RedundantAssignment
        static void SetNull<T>(ref T o) where T : class
        {
            o = null;
        }
    }
}

