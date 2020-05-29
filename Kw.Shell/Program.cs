using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Kw.Aspects;
using Kw.Aspects.Interceptors;
using Kw.Common;
using Kw.Common.Collections;
using Kw.Common.Threading;
using Kw.WinAPI;
using Kw.Windows.Forms;
using PostSharp.Aspects;

namespace Kw.Shell
{
    //	ReSharper disable EmptyGeneralCatchClause
#pragma warning disable 4014
#pragma warning disable 0162

    partial class Program
    {
        private static void OnMouseMove(object sender, MouseEventArgs e)
        {
            Console.WriteLine("Mouse move");
        }

        [STAThread]
        public static void Main(string[] arguments)
        {
            HookManager.MouseMove += OnMouseMove;

            Application.Run();

            return;
        }
    }
}
