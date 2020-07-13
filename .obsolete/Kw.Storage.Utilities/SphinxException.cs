using System;
using System.Runtime.Serialization;

namespace Kw.Storage.Utilities
{
	/// <summary>
	/// ���������� ��� ���������� Sphinx
	/// </summary>
	[Serializable]
	public class SphinxException : Exception
	{
		public SphinxException() { }
		public SphinxException(string message) : base(message) { }
		public SphinxException(string message, Exception innerException) : base(message, innerException) { }
		protected SphinxException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
