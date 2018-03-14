using System;
using System.Runtime.Serialization;

namespace Kw.Storage.Utilities
{
	/// <summary>
	/// Информирует о недоступности одного или нескольких критически важных серверов.
	/// </summary>
	[Serializable]
	public class ServerAvailabilityException : Exception
	{
		public AvailabilityTarget Target { get; private set; }

		public ServerAvailabilityException(AvailabilityTarget target) : base(String.Format("{0} server is unavailable", target))
		{
			Target = target;
		}

		protected ServerAvailabilityException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
