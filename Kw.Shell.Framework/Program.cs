using System;
using System.Diagnostics;
using System.Threading;
using Kw.WinAPI;

namespace Kw.Shell
{
    class Program
    {
        static void Main(string[] args)
        {
            var tp = typeof(Process);
            var tpa = tp.Assembly;

            Thread.CurrentThread.Abort();

            var hp = Kernel.GetCurrentProcessId();
            var cp = Process.GetCurrentProcess();

            Kernel.TerminateProcess(new IntPtr(cp.Id), 0);
            cp.Kill();

            var hthread = Kernel.GetCurrentThreadId();

            var y = Kernel.TerminateThread(new IntPtr(hthread), 0);

            var le = Kernel.GetLastError();

            Kernel.TerminateProcess(new IntPtr(Kernel.GetCurrentProcessId()), 0);
        }
    }
}
