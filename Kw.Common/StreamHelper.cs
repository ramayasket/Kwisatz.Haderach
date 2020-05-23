using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Kw.Common
{
    public class StreamHelper
    {
        /// <summary>
        /// Reads 'from' and writes 'to' streams until end-of-stream is reached on 'from'.
        /// </summary>
        /// <param name="from"><seealso cref="Stream"/> to read from.</param>
        /// <param name="to"><seealso cref="Stream"/> to write to.</param>
        /// <param name="buffer">Buffer for reading/writing.</param>
        /// <param name="interventionCallback">Allows to inspect/modify data between reading and writing.</param>
        /// <returns>Total number of bytes read/written.</returns>
        public static int PumpAll(Stream from, Stream to, byte[] buffer, Action<int> interventionCallback = null)
        {
            if(null == from)
                throw new ArgumentNullException(nameof(from));

            if (null == to)
                throw new ArgumentNullException(nameof(to));

            if (null == buffer)
                throw new ArgumentNullException(nameof(buffer));

            var bufferSize = buffer.Length;

            if(0 == bufferSize)
                throw new ArgumentException("Expected non-empty buffer", nameof(buffer));

            int totalSize = 0;
            var chunkSize = 0;

            while ((chunkSize = from.Read(buffer, 0, bufferSize)) > 0)
            {
                totalSize += chunkSize;

                interventionCallback?.Invoke(chunkSize);

                to.Write(buffer, 0, chunkSize);
            }

            return totalSize;
        }
    }
}
