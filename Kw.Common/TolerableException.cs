using System;
using System.Runtime.Serialization;

namespace Kw.Common
{
	[Serializable]
	public class TolerableException : Exception
	{
		public Exception[] InnerExceptions { get; private set; }
		public TolerableException(string message, Exception inner = null) : base(message, inner) { }

		public TolerableException(string message, Exception[] inner) : base(message)
		{
			InnerExceptions = inner;
		}

		protected TolerableException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}

