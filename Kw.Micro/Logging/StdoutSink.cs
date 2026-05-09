using Kw.Common;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace Kw.Micro.Logging
{
    public static class StdoutSinkExtensions
    {
        public static LoggerConfiguration Stdout(this LoggerSinkConfiguration configuration)
            => configuration.Sink(new StdoutSink());
    }

    public class StdoutSink : ILogEventSink
    {
        static readonly object _this = new();

        public void Emit(LogEvent le)
        {
            const ConsoleColor gray = ConsoleColor.Gray;

            string message = le.ReparseMessage()!;

            lock (_this)
            {
                Write("[", gray);
                Write($"{le.Timestamp.DateTime:T} ", gray);

                ConsoleColor levelcolor = le.Level switch
                {
                    LogEventLevel.Debug => gray,
                    LogEventLevel.Verbose => gray,
                    LogEventLevel.Warning => ConsoleColor.Yellow,
                    LogEventLevel.Error => ConsoleColor.Red,
                    LogEventLevel.Fatal => ConsoleColor.Red,
                    _ => ConsoleColor.White,
                };

                Write($"{Strlevel((LL)le.Level)}", levelcolor);

                Write("] ", gray);

                Write(FullMessage(message, le.Exception));

                Console.WriteLine();
            }
        }

        static string FullMessage(string? mt, Exception? error)
        {
            string output = "";
            mt ??= "";

            output += mt;

            if (null != error)
            {
                if (output.Length > 0)
                    output += " | ";

                output += error.GetType().AsString();

                output += $" | {error.Message}";
            }

            return output;
        }

        static string Strlevel(LL l)
        {
            return l switch
            {
                LL.V => "VRB",
                LL.D => "DBG",
                LL.I => "INF",
                LL.W => "WRN",
                LL.E => "ERR",
                LL.F => "FTL",
                _ => "   ",
            };
        }

        static void Write(string s, ConsoleColor cc = ConsoleColor.White)
        {
            Console.ForegroundColor = cc;
            Console.Write(s);
            Console.ResetColor();
        }
    }
}
