/*using System;

namespace Surging.Core.CPlatform.Logging
{
    /// <summary>
    /// 日志等级。
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// 追踪。
        /// </summary>
        Trace = 0,

        /// <summary>
        /// 调试。
        /// </summary>
        Debug = 1,

        /// <summary>
        /// 信息。
        /// </summary>
        Information = 2,

        /// <summary>
        /// 警告。
        /// </summary>
        Warning = 3,

        /// <summary>
        /// 错误。
        /// </summary>
        Error = 4,

        /// <summary>
        /// 致命错误。
        /// </summary>
        Fatal = 5
    }

    /// <summary>
    /// 一个抽象的日志记录器。
    /// </summary>
    /// <typeparam name="T">日志记录器类型。</typeparam>
    public interface ILogger<T> : ILogger
    {
    }

    /// <summary>
    /// 一个抽象的日志记录器。
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// 判断日志记录器是否开启。
        /// </summary>
        /// <param name="level">日志等级。</param>
        /// <returns>如果开启返回true，否则返回false。</returns>
        bool IsEnabled(LogLevel level);

        /// <summary>
        /// 记录日志。
        /// </summary>
        /// <param name="level">日志等级。</param>
        /// <param name="message">消息。</param>
        /// <param name="exception">异常。</param>
        /// <returns>任务。</returns>
        void Log(LogLevel level, string message, Exception exception = null);
    }

    /// <summary>
    /// 日志记录器扩展。
    /// </summary>
    public static class LoggerExtensions
    {
        /// <summary>
        /// 追踪。
        /// </summary>
        /// <param name="logger">日志记录器。</param>
        /// <param name="message">消息。</param>
        /// <param name="exception">异常信息。</param>
        public static void Trace(this ILogger logger, string message, Exception exception = null)
        {
            logger.Log(LogLevel.Trace, message, exception);
        }

        /// <summary>
        /// 调试。
        /// </summary>
        /// <param name="logger">日志记录器。</param>
        /// <param name="message">消息。</param>
        /// <param name="exception">异常信息。</param>
        public static void Debug(this ILogger logger, string message, Exception exception = null)
        {
            logger.Log(LogLevel.Debug, message, exception);
        }

        /// <summary>
        /// 信息。
        /// </summary>
        /// <param name="logger">日志记录器。</param>
        /// <param name="message">消息。</param>
        /// <param name="exception">异常信息。</param>
        public static void Information(this ILogger logger, string message, Exception exception = null)
        {
            logger.Log(LogLevel.Information, message, exception);
        }

        /// <summary>
        /// 警告。
        /// </summary>
        /// <param name="logger">日志记录器。</param>
        /// <param name="message">消息。</param>
        /// <param name="exception">异常信息。</param>
        public static void Warning(this ILogger logger, string message, Exception exception = null)
        {
            logger.Log(LogLevel.Warning, message, exception);
        }

        /// <summary>
        /// 错误。
        /// </summary>
        /// <param name="logger">日志记录器。</param>
        /// <param name="message">消息。</param>
        /// <param name="exception">异常信息。</param>
        public static void Error(this ILogger logger, string message, Exception exception = null)
        {
            logger.Log(LogLevel.Error, message, exception);
        }

        /// <summary>
        /// 失败。
        /// </summary>
        /// <param name="logger">日志记录器。</param>
        /// <param name="message">消息。</param>
        /// <param name="exception">异常信息。</param>
        public static void Fatal(this ILogger logger, string message, Exception exception = null)
        {
            logger.Log(LogLevel.Fatal, message, exception);
        }
    }
}*/