/*
 * Logger.cs
 *
 * The MIT License
 *
 * Copyright (c) 2013-2015 sta.blockhead
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Diagnostics;
using System.IO;

namespace WebSocketCore
{
    /// <summary>
    /// Provides a set of methods and properties for logging.
    /// </summary>
    public class Logger
    {
        #region 字段

        /// <summary>
        /// Defines the _file
        /// </summary>
        private volatile string _file;

        /// <summary>
        /// Defines the _level
        /// </summary>
        private volatile LogLevel _level;

        /// <summary>
        /// Defines the _output
        /// </summary>
        private Action<LogData, string> _output;

        /// <summary>
        /// Defines the _sync
        /// </summary>
        private object _sync;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger"/> class.
        /// </summary>
        public Logger()
      : this(LogLevel.Error, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger"/> class.
        /// </summary>
        /// <param name="level">The level<see cref="LogLevel"/></param>
        public Logger(LogLevel level)
      : this(level, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger"/> class.
        /// </summary>
        /// <param name="level">The level<see cref="LogLevel"/></param>
        /// <param name="file">The file<see cref="string"/></param>
        /// <param name="output">The output<see cref="Action{LogData, string}"/></param>
        public Logger(LogLevel level, string file, Action<LogData, string> output)
        {
            _level = level;
            _file = file;
            _output = output ?? defaultOutput;
            _sync = new object();
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the current path to the log file.
        /// </summary>
        public string File
        {
            get
            {
                return _file;
            }

            set
            {
                lock (_sync)
                {
                    _file = value;
                    Warn(
                      String.Format("The current path to the log file has been changed to {0}.", _file));
                }
            }
        }

        /// <summary>
        /// Gets or sets the current logging level.
        /// </summary>
        public LogLevel Level
        {
            get
            {
                return _level;
            }

            set
            {
                lock (_sync)
                {
                    _level = value;
                    Warn(String.Format("The current logging level has been changed to {0}.", _level));
                }
            }
        }

        /// <summary>
        /// Gets or sets the current output action used to output a log.
        /// </summary>
        public Action<LogData, string> Output
        {
            get
            {
                return _output;
            }

            set
            {
                lock (_sync)
                {
                    _output = value ?? defaultOutput;
                    Warn("The current output action has been changed.");
                }
            }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// Outputs <paramref name="message"/> as a log with <see cref="LogLevel.Debug"/>.
        /// </summary>
        /// <param name="message">The message<see cref="string"/></param>
        public void Debug(string message)
        {
            if (_level > LogLevel.Debug)
                return;

            output(message, LogLevel.Debug);
        }

        /// <summary>
        /// Outputs <paramref name="message"/> as a log with <see cref="LogLevel.Error"/>.
        /// </summary>
        /// <param name="message">The message<see cref="string"/></param>
        public void Error(string message)
        {
            if (_level > LogLevel.Error)
                return;

            output(message, LogLevel.Error);
        }

        /// <summary>
        /// Outputs <paramref name="message"/> as a log with <see cref="LogLevel.Fatal"/>.
        /// </summary>
        /// <param name="message">The message<see cref="string"/></param>
        public void Fatal(string message)
        {
            output(message, LogLevel.Fatal);
        }

        /// <summary>
        /// Outputs <paramref name="message"/> as a log with <see cref="LogLevel.Info"/>.
        /// </summary>
        /// <param name="message">The message<see cref="string"/></param>
        public void Info(string message)
        {
            if (_level > LogLevel.Info)
                return;

            output(message, LogLevel.Info);
        }

        /// <summary>
        /// Outputs <paramref name="message"/> as a log with <see cref="LogLevel.Trace"/>.
        /// </summary>
        /// <param name="message">The message<see cref="string"/></param>
        public void Trace(string message)
        {
            if (_level > LogLevel.Trace)
                return;

            output(message, LogLevel.Trace);
        }

        /// <summary>
        /// Outputs <paramref name="message"/> as a log with <see cref="LogLevel.Warn"/>.
        /// </summary>
        /// <param name="message">The message<see cref="string"/></param>
        public void Warn(string message)
        {
            if (_level > LogLevel.Warn)
                return;

            output(message, LogLevel.Warn);
        }

        /// <summary>
        /// The defaultOutput
        /// </summary>
        /// <param name="data">The data<see cref="LogData"/></param>
        /// <param name="path">The path<see cref="string"/></param>
        private static void defaultOutput(LogData data, string path)
        {
            var log = data.ToString();
            Console.WriteLine(log);
            if (path != null && path.Length > 0)
                writeToFile(log, path);
        }

        /// <summary>
        /// The writeToFile
        /// </summary>
        /// <param name="value">The value<see cref="string"/></param>
        /// <param name="path">The path<see cref="string"/></param>
        private static void writeToFile(string value, string path)
        {
            using (var writer = new StreamWriter(path, true))
            using (var syncWriter = TextWriter.Synchronized(writer))
                syncWriter.WriteLine(value);
        }

        /// <summary>
        /// The output
        /// </summary>
        /// <param name="message">The message<see cref="string"/></param>
        /// <param name="level">The level<see cref="LogLevel"/></param>
        private void output(string message, LogLevel level)
        {
            lock (_sync)
            {
                if (_level > level)
                    return;

                LogData data = null;
                try
                {
                    data = new LogData(level, new StackFrame(2, true), message);
                    _output(data, _file);
                }
                catch (Exception ex)
                {
                    data = new LogData(LogLevel.Fatal, new StackFrame(0, true), ex.Message);
                    Console.WriteLine(data.ToString());
                }
            }
        }

        #endregion 方法
    }
}