using Kw.Common;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace Kw.Common.Communications
{
    public class HttpClient : WebClient, INetworkResourceWebClient
    {
        private static readonly int CommonTimeout;
        private static readonly bool Reporting;

        public string ProxyUri
        {
            get { return (null != Proxy) ? Proxy.ToString() : null; }
            set { Proxy = (null == value) ? null : new WebProxy(value); }
        }

        static HttpClient()
        {
            ServicePointManager.DefaultConnectionLimit = 2048;

            CommonTimeout = AppConfig.Setting("http_timeout", 60000);
            Reporting = AppConfig.Setting("http_reporting", false);
        }

        private static bool proxy_reported = false;

        public HttpClient()
        {
            Headers[HttpRequestHeader.AcceptEncoding] = "gzip";

            var proxy = AppConfig.Setting("http_proxy");

            if (!string.IsNullOrEmpty(proxy))
            {
                Proxy = new WebProxy(proxy);

                if(!proxy_reported)
                {
                    proxy_reported = true;
                    Qizarate.Output?.WriteLine("Proxy {0}", proxy);
                }
            }
        }

        public int Timeout = 0;

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address) as HttpWebRequest;

            Debug.Assert(null != request);

            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

            if(0 != Timeout)
            {
                request.Timeout = Timeout;
            }

            request.Proxy = Proxy;

            return request;
        }

        [DebuggerNonUserCode]
        public byte[] DownloadBytes(string url, int timeout = 0, int stumbling = 0)
        {
            byte[] result = null;

            Timeout = (timeout == 0) ? CommonTimeout : timeout;

            var sw = Stopwatch.StartNew();

            try
            {
                result = DownloadData(url);
                sw.Stop();

                ReportOperation(true, sw.Elapsed, url);

                return result;
            }
            catch (Exception x)
            {
                sw.Stop();
                ReportOperation(false, sw.Elapsed, x.Message);
                throw;
            }
        }

        private static object _this = new object();

        private static void ReportOperation(bool ok, TimeSpan ts, string url)
        {
            if (!Reporting)
                return;

            lock (_this)
            {
                using (var w = new StreamWriter("HttpClient.log", true))
                {
                    w.WriteLine("{0} ({1}) {2}", ok ? "ok":"nok", (long)ts.TotalMilliseconds, url);
                }
            }
        }
    }
}
