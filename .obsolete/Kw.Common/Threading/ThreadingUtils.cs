using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kw.Common.Threading
{
    public static class ThreadingUtils
    {
        public static void DoubleCheck(Func<bool> condition, object locker, Action deed)
        {
            if (condition())
            {
                lock (locker)
                {
                    if (condition())
                    {
                        deed();
                    }
                }
            }
        }
    }
}
