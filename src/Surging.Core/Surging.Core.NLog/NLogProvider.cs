using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Nlog
{
    /// <summary>
    /// Defines the <see cref="NLogProvider" />
    /// </summary>
    public class NLogProvider : ILoggerProvider
    {
        #region 字段

        /// <summary>
        /// Defines the _loggers
        /// </summary>
        private readonly ConcurrentDictionary<string, NLogger> _loggers =
            new ConcurrentDictionary<string, NLogger>();

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="NLogProvider"/> class.
        /// </summary>
        public NLogProvider()
        {
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The CreateLogger
        /// </summary>
        /// <param name="categoryName">The categoryName<see cref="string"/></param>
        /// <returns>The <see cref="ILogger"/></returns>
        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, CreateLoggerImplementation);
        }

        /// <summary>
        /// The Dispose
        /// </summary>
        public void Dispose()
        {
            _loggers.Clear();
        }

        /// <summary>
        /// The CreateLoggerImplementation
        /// </summary>
        /// <param name="name">The name<see cref="string"/></param>
        /// <returns>The <see cref="NLogger"/></returns>
        private NLogger CreateLoggerImplementation(string name)
        {
            return new NLogger(name);
        }

        #endregion 方法
    }
}