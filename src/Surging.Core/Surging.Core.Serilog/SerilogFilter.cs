using Serilog.Core;
using Serilog.Events;

namespace Surging.Core.Serilog
{
    public class SerilogVerboseFilter : ILogEventFilter
    {
        public bool IsEnabled(LogEvent logEvent)
        {
            return logEvent.Level == LogEventLevel.Verbose;
        }

    }

    public class SerilogDebugFilter : ILogEventFilter
    {
        public bool IsEnabled(LogEvent logEvent)
        {
            return logEvent.Level == LogEventLevel.Debug;
        }

    }

    public class SerilogErrorFilter : ILogEventFilter
    {
        public bool IsEnabled(LogEvent logEvent)
        {
            return logEvent.Level == LogEventLevel.Error;
        }
    }

    public class SerilogFatalFilter : ILogEventFilter
    {
        public bool IsEnabled(LogEvent logEvent)
        {
            return logEvent.Level == LogEventLevel.Fatal;
        }
    }

    public class SerilogInformationFilter : ILogEventFilter
    {
        public bool IsEnabled(LogEvent logEvent)
        {
            return logEvent.Level == LogEventLevel.Information;
        }
    }

    public class SerilogWarningFilter : ILogEventFilter
    {
        public bool IsEnabled(LogEvent logEvent)
        {
            return logEvent.Level == LogEventLevel.Warning;
        }
    }
}
