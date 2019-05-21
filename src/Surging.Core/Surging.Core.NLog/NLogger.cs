using Microsoft.Extensions.Logging;

using System;
using System.Runtime.CompilerServices;

namespace Surging.Core.Nlog
{
    public class NLogger : Microsoft.Extensions.Logging.ILogger
    {
        private readonly NLog.Logger _log;
        public NLogger(string name)
        { 
            _log = NLog.LogManager.GetLogger(name);
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return NoopDisposable.Instance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Critical:
                    return _log.IsFatalEnabled;
                case LogLevel.Debug:
                    return _log.IsDebugEnabled;
                case LogLevel.Trace:
                    return _log.IsTraceEnabled;
                case LogLevel.Error:
                    return _log.IsErrorEnabled;
                case LogLevel.Information:
                    return _log.IsInfoEnabled;
                case LogLevel.Warning:
                    return _log.IsWarnEnabled;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel));
            }
        }

        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state,
            Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }
            string message = null;
            if (null != formatter)
            {
                message = formatter(state, exception);
            }
            if (!string.IsNullOrEmpty(message) || exception != null)
            {
                switch (logLevel)
                {
                    case LogLevel.Critical:
                        _log.Fatal(message);
                        break;
                    case LogLevel.Debug:
                        _log.Debug(message);
                        break;
                    case LogLevel.Trace:
                        _log.Trace(message);
                        break;
                    case LogLevel.Error:
                        _log.Error(message, exception,null);
                        break;
                    case LogLevel.Information:
                        _log.Info(message);
                        break;
                    case LogLevel.Warning:
                        _log.Warn(message);
                        break;
                    default:
                        _log.Warn($"遇到未知日志级别{logLevel}");
                        _log.Info(message, exception,null);
                        break;
                }
            }
        }

        private class NoopDisposable : IDisposable
        {
            public static NoopDisposable Instance = new NoopDisposable();

            public void Dispose()
            {
               
            }
        }
    }
}
