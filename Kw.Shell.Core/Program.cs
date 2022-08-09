using Kw.WinAPI;
using Kw.Windows.Forms;
using Newtonsoft.Json;
using Syncfusion.Licensing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kw.Common;
using Kw.Common.ZSpitz;

namespace Kw.Shell
{
    //    ReSharper disable EmptyGeneralCatchClause
#pragma warning disable 4014
#pragma warning disable 0162

    public class SourceClass
    {
        public string Name { get; set; }
        public int Value { get; set; }

        public object Payload { get; set; }
        public SourceClass Child { get; set; }
    }

    public static partial class Program
    {
        private static void OnMouseMove(object sender, MouseEventArgs e)
        {
        }

        private static string RenameOpn(string s) => s.StartsWith("Opn.") ? "Kw." + s.Substring(4) : s;

        [STAThread]
        public static async Task Main(string[] arguments)
        {
            var xx = await ExecutionTimings.Measure(async () => await Task.Delay(1000));

            int[] ieni = { 1, 2, 3 };

            //ieni = null;

            var zlp = (5).Out(ieni);

            Expression<Func<object, string>> xxx = x => x.ToString();

            var xxxs = xxx.ToString("Factory methods", "C#");

            var mapPath = new AppSetting("mapPath");

            object[] src = null;
            var s = src.SafeSelect(x => x.ToString());

            for (int num = 2; num < 1000; num++)
            {
                var factors = num.Factorize();
                var sfactors = factors.Length == 0 ? "*" : string.Join(" * ", factors);
                Console.WriteLine($"{num}: {sfactors}");
            }

            return;

            //var renamed = TreeRenamer.Rename(@"C:\home\dev\Kwisatz.Haderach.Golden\", RenameOpn);

            //Qizarate.Output?.WriteLine(true);

            SyncfusionLicenseProvider.RegisterLicense("Mjg5ODUyQDMxMzgyZTMyMmUzMFhUZ3luM05aT1RPeCtNSVFwT0pqM1p5Y2puWldwZHdUeSszQlVJL2FjNFE9");
            var x = SyncfusionLicenseProvider.ValidateLicense(Platform.WPF, out string xs);

        }

        //private static void HookManagerOnMouseMoveExt(object? sender, MouseEventExtArgs e)
        //{
        //    //Console.WriteLine($"Mouse move: X:{e.X} Y:{e.Y}");
        //    e.Handled = true;
        //}
    }
}
