using System;

namespace Kw.Common.Communications
{
    [Serializable]
    public class VoidResponse : SerialExchangeResponse
    {
        public VoidResponse() { }
        public VoidResponse(Exception x) : base(x) { }
    }
}

