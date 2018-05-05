using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Kw.WinAPI
{
	internal sealed class StreamName : IDisposable
	{
		private static readonly SafeHGlobalHandle InvalidBlock = SafeHGlobalHandle.Invalid();

		/// <summary>
		/// Returns the handle to the block of memory.
		/// </summary>
		/// <value>
		/// The <see cref="SafeHGlobalHandle"/> representing the block of memory.
		/// </value>
		public SafeHGlobalHandle MemoryBlock { get; private set; } = InvalidBlock;

		/// <summary>
		/// Performs application-defined tasks associated with freeing, 
		/// releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			if (!MemoryBlock.IsInvalid)
			{
				MemoryBlock.Dispose();
				MemoryBlock = InvalidBlock;
			}
		}

		/// <summary>
		/// Ensures that there is sufficient memory allocated.
		/// </summary>
		/// <param name="capacity">
		/// The required capacity of the block, in bytes.
		/// </param>
		/// <exception cref="OutOfMemoryException">
		/// There is insufficient memory to satisfy the request.
		/// </exception>
		public void EnsureCapacity(int capacity)
		{
			int currentSize = MemoryBlock.IsInvalid ? 0 : MemoryBlock.Size;
			if (capacity > currentSize)
			{
				if (0 != currentSize) currentSize <<= 1;
				if (capacity > currentSize) currentSize = capacity;

				if (!MemoryBlock.IsInvalid) MemoryBlock.Dispose();
				MemoryBlock = SafeHGlobalHandle.Allocate(currentSize);
			}
		}

		/// <summary>
		/// Reads the Unicode string from the memory block.
		/// </summary>
		/// <param name="length">
		/// The length of the string to read, in characters.
		/// </param>
		/// <returns>
		/// The string read from the memory block.
		/// </returns>
		public string ReadString(int length)
		{
			if (0 >= length || MemoryBlock.IsInvalid) return null;
			if (length > MemoryBlock.Size) length = MemoryBlock.Size;
			return Marshal.PtrToStringUni(MemoryBlock.DangerousGetHandle(), length);
		}

		/// <summary>
		/// Reads the string, and extracts the stream name.
		/// </summary>
		/// <param name="length">
		/// The length of the string to read, in characters.
		/// </param>
		/// <returns>
		/// The stream name.
		/// </returns>
		public string ReadStreamName(int length)
		{
			string name = ReadString(length);
			if (!string.IsNullOrEmpty(name))
			{
				// Name is of the format ":NAME:$DATA\0"
				int separatorIndex = name.IndexOf(AdsUtils.StreamSeparator, 1);
				if (-1 != separatorIndex)
				{
					name = name.Substring(1, separatorIndex - 1);
				}
				else
				{
					// Should never happen!
					separatorIndex = name.IndexOf('\0');
					if (1 < separatorIndex)
					{
						name = name.Substring(1, separatorIndex - 1);
					}
					else
					{
						name = null;
					}
				}
			}

			return name;
		}
	}
}
