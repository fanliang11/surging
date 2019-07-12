using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Surging.Core.Log4net
{
    /// <summary>
    /// Defines the <see cref="Log4NetProvider" />
    /// </summary>
    public class Log4NetProvider : ILoggerProvider
    {
        #region 字段

        /// <summary>
        /// Defines the _log4NetConfigFile
        /// </summary>
        private readonly string _log4NetConfigFile;

        /// <summary>
        /// Defines the _loggers
        /// </summary>
        private readonly ConcurrentDictionary<string, Log4NetLogger> _loggers =
            new ConcurrentDictionary<string, Log4NetLogger>();

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="Log4NetProvider"/> class.
        /// </summary>
        /// <param name="log4NetConfigFile">The log4NetConfigFile<see cref="string"/></param>
        public Log4NetProvider(string log4NetConfigFile)
        {
            _log4NetConfigFile = log4NetConfigFile;
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
        /// The Parselog4NetConfigFile
        /// </summary>
        /// <param name="filename">The filename<see cref="string"/></param>
        /// <returns>The <see cref="XmlElement"/></returns>
        private static XmlElement Parselog4NetConfigFile(string filename)
        {
            XmlDocument log4netConfig = new XmlDocument();
            var stream = File.OpenRead(filename);
            log4netConfig.Load(stream);
            stream.Close();
            return log4netConfig["log4net"];
        }

        /// <summary>
        /// The CreateLoggerImplementation
        /// </summary>
        /// <param name="name">The name<see cref="string"/></param>
        /// <returns>The <see cref="Log4NetLogger"/></returns>
        private Log4NetLogger CreateLoggerImplementation(string name)
        {
            return new Log4NetLogger(name, Parselog4NetConfigFile(_log4NetConfigFile));
        }

        #endregion 方法
    }
}