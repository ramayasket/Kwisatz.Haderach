using System.IO;
using System.IO.Compression;

namespace Kw.Common
{
    /// <summary>
    /// GZip compression/decompression of byte arrays.
    /// </summary>
    public static class GZipHelper
    {
        /// <summary>
        /// Compresses byte array.
        /// </summary>
        public static byte[] Pack(this byte[] input)
        {
            using (var compressedStream = new MemoryStream())
                using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress)) {
                    zipStream.Write(input, 0, input.Length);
                    zipStream.Close();
                    var output = compressedStream.ToArray();
                    return output;
                }
        }

        /// <summary>
        /// Decompresses byte array.
        /// </summary>
        public static byte[] Unpack(this byte[] input)
        {
            using (var compressedStream = new MemoryStream(input))
                using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                    using (var resultStream = new MemoryStream()) {
                        zipStream.CopyTo(resultStream);
                        var output = resultStream.ToArray();
                        return output;
                    }
        }
    }
}
