using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kw.Common
{
    /// TODO XML comments
    public static class ExceptionExtensions
    {
        public static string GetFullMessage(this Exception x)
        {
            var parts = new List<string>();
            var current = x;

            while (null != current)
            {
                parts.Add(current.Message);
                current = current.InnerException;
            }

            return string.Join(" :: ", parts.ToArray());
        }
    }
}

