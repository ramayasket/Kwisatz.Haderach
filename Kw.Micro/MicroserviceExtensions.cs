using Microsoft.Extensions.Configuration;
using Serilog.Events;
using System.Collections.Generic;

namespace Kw.Micro
{
    public static class MicroserviceExtensions
    {
        public static IConfigurationSection GetSection<T>(this ConfigurationManager man) where T : class
            => man.GetSection(typeof(T).Name);

        public static Exception[] Unwrap(this Exception x)
        {
            List<Exception> errors = [];

            if (x is AggregateException ax)
            {
                errors.AddRange(ax.InnerExceptions);
            }
            else
            {
                errors.Add(x);

                if (null != x.InnerException)
                    errors.Add(x.InnerException);
            }

            return errors.ToArray();
        }

        public static string? Unquote(this LogEventPropertyValue v)
        {
            if (null == v)
                return null;

            string s = $"{v}";
            return s.Substring(1, s.Length - 2);
        }
    }
}
