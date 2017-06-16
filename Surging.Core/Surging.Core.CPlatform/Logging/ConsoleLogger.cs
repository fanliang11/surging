/*using System;

namespace Surging.Core.CPlatform.Logging
{
    public class ConsoleLogger<T> : ConsoleLogger, ILogger<T>
    {
    }

    public class ConsoleLogger : ILogger
    {
        #region Implementation of ILogger

        /// <summary>
        /// 判断日志记录器是否开启。
        /// </summary>
        /// <param name="level">日志等级。</param>
        /// <returns>如果开启返回true，否则返回false。</returns>
        public bool IsEnabled(LogLevel level)
        {
            return (int)level > 2;
        }

        /// <summary>
        /// 记录日志。
        /// </summary>
        /// <param name="level">日志等级。</param>
        /// <param name="message">消息。</param>
        /// <param name="exception">异常。</param>
        /// <returns>任务。</returns>
        public void Log(LogLevel level, string message, Exception exception = null)
        {
            Console.ResetColor();
            var color = Console.ForegroundColor;

            switch (level)
            {
                case LogLevel.Trace:
                    color = ConsoleColor.DarkGray;
                    break;

                case LogLevel.Debug:
                    color = ConsoleColor.Gray;
                    break;

                case LogLevel.Information:
                    color = ConsoleColor.DarkBlue;
                    break;

                case LogLevel.Warning:
                    color = ConsoleColor.Yellow;
                    break;

                case LogLevel.Error:
                    color = ConsoleColor.DarkRed;
                    break;

                case LogLevel.Fatal:
                    color = ConsoleColor.Red;
                    break;
            }

            Console.ForegroundColor = color;

            Console.WriteLine($"level：{level}");
            Console.WriteLine($"message：{message}");
            if (exception != null)
                Console.WriteLine($"exception：{exception}");
            Console.WriteLine("========================================");

            Console.ResetColor();
        }

        #endregion Implementation of ILogger
    }
}*/