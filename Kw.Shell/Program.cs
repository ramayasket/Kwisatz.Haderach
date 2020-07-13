using Kw.Aspects;
using Kw.Aspects.Interceptors;
using Kw.Common;
using Kw.Common.Collections;
using Kw.Common.Threading;
using Kw.WinAPI;
using Kw.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PostSharp.Aspects;
using Syncfusion.Licensing;
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
            Console.WriteLine("Mouse move");
        }

        [STAThread]
        public static void Main(string[] arguments)
        {
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

            jdc = jd["Child"];
            jd["Payload"] = 80053;
            var jdp = jd.Payload;

            var jdps = jd.GetDynamicMemberNames();
            var jdcps = jdc.GetDynamicMemberNames();

            //var jdn1 = jd.Name1;
            var jdcc = jdc.Child;


            var jds = jd.ToString();
        }
    }
}
