using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Kw.Common;

namespace Kw.Networking
{
    public class MultiChannelCURLClient : INetworkResourceWebClient
    {
        public string ProxyUri { get; set; }    //    not used here
        public string[] Proxies { get; set; }

        private class WithProxy
        {
            public CURLClient Client { get; private set; }
            public Guid Id { get; private set; }

            public WithProxy(string proxy = null)
            {
                Id = Guid.NewGuid();
                var assignment = CURLDialogue.Standard;

                if (null != proxy)
                {
                    assignment.Set(CURLOptionKey.proxy, "http://" + proxy);
                }

                Client = new CURLClient(assignment);
            }
        }

        private readonly WithProxy[] Channels;

        public MultiChannelCURLClient(params string[] proxies)
        {
            Proxies = proxies;
            var channels = new List<WithProxy>();

            foreach (var s in proxies)
            {
                var split = s.Split(':');

                if (split.Length != 2)
                    throw new IncorrectConfigurationException();

                var channel = new WithProxy(s);
                channels.Add(channel);
            }

            var noProxy = new WithProxy();
            channels.Add(noProxy);

            Channels = channels.ToArray();
        }

        public void Dispose()
        {
            foreach(var channel in Channels)
            {
                channel.Client.Dispose();
            }

            //PostDisposeCheck();
        }

        //public static void PostDisposeCheck()
        //{
        //    var tpath = Path.GetTempPath();

        //    var myTemps = Directory.EnumerateFiles(tpath, ".curl-*.tmp");

        //    if(myTemps.Any())
        //    {
        //        throw new IncorrectOperationException();
        //    }
        //}

        private DateTime BeginTime;

        public byte[] DownloadBytes(string url, int timeout = 0, int stumbling = 0)
        {
            BeginTime = DateTime.Now;

            foreach(var channel in Channels)
            {
                channel.Client.DownloadBytesAsync(url, timeout, stumbling);
            }

            var whandles = Channels.Select(channel => channel.Client.Invoker.Waitable).ToArray();
            var waited = WaitHandle.WaitAny(whandles);

            var waitedChannel = Channels[waited];
            byte[] bytes;

            foreach(var channel in Channels)
            {
                if(channel.Id != waitedChannel.Id)
                {
                    var task = channel.Client.Invoker;
                    task.Cancel();
                }
            }

            WaitHandle.WaitAll(whandles);

            var elapsed = DateTime.Now - BeginTime;

            if (0 == waitedChannel.Client.Dialogue.ExitCode)
            {
                bytes = waitedChannel.Client.Dialogue.OutputBytes;
                AppCore.WriteLine("@PX Request serviced by {0} in {1}", waitedChannel.Client.Dialogue.ProxyInfo, elapsed);
            }
            else
                throw new Win32Exception(waitedChannel.Client.Dialogue.ExitCode);

            return bytes;
        }
    }
}

