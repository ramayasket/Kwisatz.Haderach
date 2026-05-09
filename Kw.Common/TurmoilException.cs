using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Kw.Common
{
    /// <summary>
    /// Исключение для отладки. Означает некий жуткий пипец.
    /// </summary>
    [Serializable]
    public class TurmoilException : Exception
    {
        public TurmoilException() { }
        public TurmoilException(string message) : base(message) { }
        public TurmoilException(string message, Exception innerException) : base(message, innerException) { }
    }
}

