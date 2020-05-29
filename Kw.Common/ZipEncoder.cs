using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace model
{
    [ExcludeFromCodeCoverage]
    public static class ZipEncoder
    {
        private const string ENTRY = "0";

        private static byte[] MakeBuffer()
        {
            return new byte[4096];
        }

        private static byte[] ExtractBuffer(MemoryStream ms)
        {
            return ms.ToArray();
        }

        public static byte[] Pack(this byte[] input)
        {
            if (null == input)
                return null;

            using (var unpacked = new MemoryStream(input))
            {
                using (var outBuffer = new MemoryStream())
                {
                    using (var packed = new ZipOutputStream(outBuffer))
                    {
                        packed.SetLevel(9);

                        var entry = new ZipEntry(ENTRY)
                        {
                            DateTime = DateTime.Now,
                            
                        };

                        packed.PutNextEntry(entry);

                        var tmp = MakeBuffer();

                        StreamUtils.Copy(unpacked, packed, tmp);

                        packed.CloseEntry();
                        packed.IsStreamOwner = false;
                        packed.Close();
                        outBuffer.Position = 0;

                        return ExtractBuffer(outBuffer);
                    }
                }
            }
        }

        public static byte[] Unpack(this byte[] input)
        {
            if (null == input)
                return null;

            using (var packed = new MemoryStream(input))
            {
                using (var zfile = new ZipFile(packed))
                {
                    var entry = zfile.GetEntry(ENTRY);
                    var tmp = MakeBuffer();

                    using (var zip = zfile.GetInputStream(entry))
                    {
                        using (var unpacked = new MemoryStream())
                        {
                            StreamUtils.Copy(zip, unpacked, tmp);
                            return ExtractBuffer(unpacked);
                        }
                    }
                }
            }
        }
    }
}
