using System;

namespace Kw.Networking.Communications
{
	[Serializable]
	public class VoidResponse : SerialExchangeResponse
	{
		public VoidResponse() { }
		public VoidResponse(Exception x) : base(x) { }
	}
}

