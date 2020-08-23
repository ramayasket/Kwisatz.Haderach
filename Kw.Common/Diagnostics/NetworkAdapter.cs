using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Kw.Common.Diagnostics
{
    /// <summary> Методы для работы с сетевым адаптером. </summary>
    public static class NetworkAdapter
    {
        /// <summary> Включить сетевой адаптер. </summary>
        /// <param name="adapterName"> Имя сетевого адаптера. </param>
        public static void Enable(string adapterName = "Ethernet") => Switch(adapterName, true);

        /// <summary> Выключить сетевой адаптер. </summary>
        /// <param name="adapterName"> Имя сетевого адаптера. </param>
        public static void Disable(string adapterName = "Ethernet") => Switch(adapterName, false);

        private static void Switch(string adapterName, bool enabled)
        {
            if (string.IsNullOrEmpty(adapterName))
                throw new ArgumentNullException(nameof(adapterName), "Не задано имя сетевого адаптера.");

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new PlatformNotSupportedException("Windows only feature yet");

            using (var process = new Process())
            {
                process.StartInfo.FileName = "netsh";
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                string state = enabled ? "enabled" : "disabled";
                process.StartInfo.Arguments = $"interface set interface name=\"{adapterName}\" admin={state}";
                process.Start();
                process.WaitForExit();
            }
        }
    }
}
