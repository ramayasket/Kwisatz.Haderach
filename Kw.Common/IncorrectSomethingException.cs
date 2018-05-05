using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Kw.Common
{
	[Serializable]
	public class IncorrectEnvironmentException : Exception
	{
		public IncorrectEnvironmentException() { }
		public IncorrectEnvironmentException(string message) : base(message) { }
		protected IncorrectEnvironmentException(SerializationInfo info, StreamingContext context) : base(info, context) { }
		public IncorrectEnvironmentException(string message, Exception innerException) : base(message, innerException) { }
	}

	[Serializable]
	public class IncorrectDataException : Exception
	{
		public IncorrectDataException() {}
		public IncorrectDataException(string message) : base(message) {}
		protected IncorrectDataException(SerializationInfo info, StreamingContext context) : base(info, context) {}
		public IncorrectDataException(string message, Exception innerException) : base(message, innerException) {}
	}

	[Serializable]
	public class IncorrectOperationException : Exception
	{
		public IncorrectOperationException() { }
		public IncorrectOperationException(string message) : base(message) { }
		protected IncorrectOperationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
		public IncorrectOperationException(string message, Exception innerException) : base(message, innerException) { }
	}

	[Serializable]
	public class IncorrectConfigurationException : Exception
	{
		public IncorrectConfigurationException() { }
		public IncorrectConfigurationException(string message) : base(message) { }
		protected IncorrectConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
		public IncorrectConfigurationException(string message, Exception innerException) : base(message, innerException) { }
	}

	[Serializable]
	public class IncorrectTypeException : Exception
	{
		public IncorrectTypeException() { }
		public IncorrectTypeException(string message) : base(message) { }
		protected IncorrectTypeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
		public IncorrectTypeException(string message, Exception innerException) : base(message, innerException) { }
	}

	[Serializable]
	public class NotReachableException : Exception
	{
		public NotReachableException() : base("Not supposed to reach this exception.") { }
		protected NotReachableException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}

