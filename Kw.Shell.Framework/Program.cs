using System;
using System.Diagnostics;
using System.Threading;
using Kw.Common;
using Kw.WinAPI;

namespace Kw.Shell
{
    class Program
    {
        static void Main(string[] args)
        {
            var m = FrameworkUtils.GetStackMethod(0);
        }
    }
}
