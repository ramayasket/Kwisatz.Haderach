using Kw.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Kw.Common.Communications
{
    public interface INetworkResourceWebClient : IDisposable
    {
        string ProxyUri { get; set; }
        byte[] DownloadBytes(string url, int timeout = 0, int stumbling = 0);
    }

    public class WebClientAllocator : IDisposable
    {
        [ThreadStatic]
        public static INetworkResourceWebClient Client;

        public WebClientAllocator(INetworkResourceWebClient client)
        {
            Client = client;
        }

        public void Dispose()
        {
            Client = null;
        }
    }

    public class NetworkResourceClient
    {
        public string Proxy { get; set; } // Временно, пока изучается вопрос отклика API TaoBao.
        public string ProxyUri { get; set; }

        static string GetResponse(string url)
        {
            var rq = (HttpWebRequest)WebRequest.Create(url);

            rq.Timeout = 60000;
            rq.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip");
            rq.AutomaticDecompression = DecompressionMethods.GZip;

            var response = (HttpWebResponse)rq.GetResponse();
            var responseStream = response.GetResponseStream();

            if (null != responseStream && null != response.CharacterSet)
            {
                var sr = new StreamReader(responseStream, Encoding.GetEncoding(response.CharacterSet));
                var responseText = sr.ReadToEnd();
                responseStream.Close();

                var status = response.StatusCode;
                response.Close();

                if (HttpStatusCode.OK == status)
                {
                    return responseText;
                }
            }

            throw new WebException("Error in response.");
        }

        [DebuggerNonUserCode]
        public virtual string Execute(NetworkResourceRequest request)
        {
            using (var wc = GetClientInternal())
            {
                wc.ProxyUri = ProxyUri;
                
                var rq = request.MakeRequest();

                if (!string.IsNullOrWhiteSpace(Proxy))
                    rq = Proxy + rq.Substring(rq.IndexOf('?'));

                request.RequestUri = rq;

                var b = wc.DownloadBytes(rq);

                if (null == b)
                {
                    return "";
                }

                return Encoding.UTF8.GetString(b);
            }
        }

        private INetworkResourceWebClient GetClientInternal()
        {
            var client = WebClientAllocator.Client ?? GetClient();
            return client;
        }

        public static INetworkResourceWebClient GetClient()
        {
            return new HttpClient();
        }
    }
}

