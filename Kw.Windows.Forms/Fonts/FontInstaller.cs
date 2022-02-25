using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Kw.Common;
using Microsoft.Win32;

namespace Kw.Windows.Forms.Fonts
{
    /// <summary>
    /// Some fonts known to Kw.Windows.Forms.
    /// </summary>
    public enum KwFontFace
    {
        NOTOMONO = 1,
    }

    /// <summary>
    /// Installs font(s) required for some components (i.e. FastColoredTextBox).
    /// Works under Windows.
    /// </summary>
    public static class FontInstaller
    {
        [DllImport("gdi32", EntryPoint = "AddFontResource")]
        public static extern int AddFontResourceA(string lpFileName);
        [DllImport("gdi32.dll")]
        private static extern int AddFontResource(string lpszFilename);
        [DllImport("gdi32.dll")]
        private static extern int CreateScalableFontResource(uint fdwHidden, string lpszFontRes, string lpszFontFile, string lpszCurrentPath);

        public static void InstallFont(KwFontFace face)
        {
            if(Environment.OSVersion.Platform != PlatformID.Win32NT)
                throw new InvalidOperationException("InstallFont is for Win32NT only.");

            string name, resourceName;

            switch (face)
            {
                case KwFontFace.NOTOMONO:
                    name = "Noto Mono";
                    resourceName = "notomono.ttf";
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"Font {face} is not supported.");
            }

            var ifc = new InstalledFontCollection();

            var found = ifc.Families.SingleOrDefault(f => f.Name == name);

            if (null != found)
                return;

            using (var tf = new TemporaryFile())
            {
                using (var resourceStream = Qizarate.GetResource("Fonts." + resourceName, Assembly.GetExecutingAssembly()))
                {
                    using (var outStream = tf.Create())
                    {
                        resourceStream.CopyTo(outStream);
                    }
                }

                // Creates the full path where your font will be installed
                var destination = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), resourceName);

                if (!File.Exists(destination))
                {
                    // Copies font to destination
                    File.Copy(tf.Path, destination);

                    // Retrieves font name
                    // Makes sure you reference System.Drawing
                    PrivateFontCollection fontCol = new PrivateFontCollection();
                    fontCol.AddFontFile(destination);
                    var actualFontName = fontCol.Families[0].Name;

                    //Add font
                    AddFontResource(destination);
                    //Add registry entry   
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts", actualFontName, resourceName, RegistryValueKind.String);
                }
            }

        }
    }
}
