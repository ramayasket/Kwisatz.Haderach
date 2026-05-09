using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace Kw.Micro.Logging
{
    public static class LogUtilities
    {
        public static string ReparseMessage(this LogEvent le)
        {
            string message = $"{le.MessageTemplate}";

            if (message == "[null]")
                message = "";

            char[] chars = message.ToCharArray();

            bool inside = false;

            string current = "";

            List<string> tokens = new();

            foreach (char c in chars)
            {
                if (inside && '}' == c)
                {
                    inside = false;
                    tokens.Add(current);
                    continue;
                }
                
                if (inside)
                {
                    current += c;
                    continue;
                }

                if ('{' == c)
                {
                    inside = true;
                    current = "";
                }
            }

            foreach (string token in tokens)
            {
                string? value = le.GetPropertyValue(token);

                if (null != value)
                    message = message.Replace($"{{{token}}}", value);
            }

            return message;
        }


        public static string? GetPropertyValue(this LogEvent le, string propname) =>
            le.Properties.TryGetValue(propname, out LogEventPropertyValue? value) ? value.Unquote() : null;

        public static void Write(this ILogger? logger, LL l, string message)
        {
            logger?.Log((LogLevel)(int)l, message);
        }

        public static void Write(this ILogger? logger, Exception error, string? message = null)
        {
            Exception[] errors = error.Unwrap();

            foreach (Exception x in errors)
                logger?.Log(LogLevel.Error, x, message);
        }
    }
}
