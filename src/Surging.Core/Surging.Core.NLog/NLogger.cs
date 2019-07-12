using Microsoft.Extensions.Logging;
using System;
using System.Runtime.CompilerServices;

namespace Surging.Core.Nlog
{
    /// <summary>
    /// Defines the <see cref="NLogger" />
    /// </summary>
    public class NLogger : Microsoft.Extensions.Logging.ILogger
    {
        #region 字段

        /// <summary>
        /// Defines the _log
        /// </summary>
        private readonly NLog.Logger _log;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="NLogger"/> class.
        /// </summary>
        /// <param name="name">The name<see cref="string"/></param>
        public NLogger(string name)
        {
            _log = NLog.LogManager.GetLogger(name);
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The BeginScope
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="state">The state<see cref="TState"/></param>
        /// <returns>The <see cref="IDisposable"/></returns>
        public IDisposable BeginScope<TState>(TState state)
        {
            return NoopDisposable.Instance;
        }

        /// <summary>
        /// The IsEnabled
        /// </summary>
        /// <param name="logLevel">The logLevel<see cref="Microsoft.Extensions.Logging.LogLevel"/></param>
        /// <returns>The <see cref="bool"/></returns>
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

        /// <summary>
        /// The Log
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="logLevel">The logLevel<see cref="Microsoft.Extensions.Logging.LogLevel"/></param>
        /// <param name="eventId">The eventId<see cref="EventId"/></param>
        /// <param name="state">The state<see cref="TState"/></param>
        /// <param name="exception">The exception<see cref="Exception"/></param>
        /// <param name="formatter">The formatter<see cref="Func{TState, Exception, string}"/></param>
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
                        _log.Error(message, exception, null);
                        break;

                    case LogLevel.Information:
                        _log.Info(message);
                        break;

                    case LogLevel.Warning:
                        _log.Warn(message);
                        break;

                    default:
                        _log.Warn($"遇到未知日志级别{logLevel}");
                        _log.Info(message, exception, null);
                        break;
                }
            }
        }

        #endregion 方法

        /// <summary>
        /// Defines the <see cref="NoopDisposable" />
        /// </summary>
        private class NoopDisposable : IDisposable
        {
            #region 字段

            /// <summary>
            /// Defines the Instance
            /// </summary>
            public static NoopDisposable Instance = new NoopDisposable();

            #endregion 字段

            #region 方法

            /// <summary>
            /// The Dispose
            /// </summary>
            public void Dispose()
            {
            }

            #endregion 方法
        }
    }
}