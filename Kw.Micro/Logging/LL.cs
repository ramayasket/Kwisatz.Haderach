using Serilog.Events;

namespace Kw.Micro.Logging
{
    /// <summary>
    /// Log level: short form.
    /// </summary>
    public enum LL
    {
        V = LogEventLevel.Verbose,
        D = LogEventLevel.Debug,
        I = LogEventLevel.Information,
        W = LogEventLevel.Warning,
        E = LogEventLevel.Error,
        F = LogEventLevel.Fatal,
    }
}
