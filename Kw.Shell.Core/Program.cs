using Kw.Common;
using Kw.WinAPI;
using Kw.Windows.Forms;
using Newtonsoft.Json;
using Syncfusion.Licensing;
using System;
using System.Windows.Forms;

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

    partial class Program
    {
        private static void OnMouseMove(object sender, MouseEventArgs e)
        {
        }

        private static string RenameOpn(string s) => s.StartsWith("Opn.") ? "Kw." + s.Substring(4) : s;

        [STAThread]
        public static void Main(string[] arguments)
        {
            dynamic zlp = new SourceClass();

            zlp.Name = "zlp";

            var hthread = Kernel.GetCurrentThreadId();

            var y = Kernel.TerminateThread(new IntPtr(hthread), 0);

            var le = Kernel.GetLastError();

            Kernel.TerminateProcess(new IntPtr(Kernel.GetCurrentProcessId()), 0);

            var renamed = TreeRenamer.Rename(@"C:\home\dev\Kwisatz.Haderach.Golden\", RenameOpn);

            Qizarate.Output?.WriteLine(true);

            ////HookManager.MouseMoveExt += HookManagerOnMouseMoveExt;
            //HookManager.MouseMove += OnMouseMove;

            //Application.Run();

            //HookManager.MouseMove -= OnMouseMove;
            ////HookManager.MouseMoveExt -= HookManagerOnMouseMoveExt;

            //return;

            var writer = Console.Out;

            SyncfusionLicenseProvider.RegisterLicense("Mjg5ODUyQDMxMzgyZTMyMmUzMFhUZ3luM05aT1RPeCtNSVFwT0pqM1p5Y2puWldwZHdUeSszQlVJL2FjNFE9");
            var x = SyncfusionLicenseProvider.ValidateLicense(Platform.WPF, out string xs);

            var t = new DateTime(2020, 6, 25);
            t = DateTime.SpecifyKind(t, DateTimeKind.Utc);

            var s = new SourceClass
            {
                Name = "Ramayasket",
                Value = 8052,
                Payload = new byte[] { 1, 2, 4, 8 },
                Child = null,
            };

            var s1 = s.Child = new SourceClass {
                Name = "\\Child of Ramayasket",
                Value = 80052,
                //Payload = new byte[] { 1, 2, 4, 8 },
                Payload = t,
                Child = null,
            };

            var js = JsonConvert.SerializeObject(s);

            dynamic jd = new JDynamic(js);

            //jd.Payload = double.NaN;
            //var jdp = jd.Payload;

            var jdc = jd.Child;

            //jdc = jd["Child"];
            //jd["Payload"] = 80053;
            var jdp = jd.Payload;

            var jdps = jd.GetDynamicMemberNames();
            var jdcps = jdc.GetDynamicMemberNames();

            //var jdn1 = jd.Name1;
            var jdcc = jdc.Child;


            var jds = jd.ToString();
        }

        private static void HookManagerOnMouseMoveExt(object? sender, MouseEventExtArgs e)
        {
            //Console.WriteLine($"Mouse move: X:{e.X} Y:{e.Y}");
            e.Handled = true;
        }
    }
}
