/*
 * LogData.cs
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
using System.Text;

namespace WebSocketCore
{
    /// <summary>
    /// Represents a log data used by the <see cref="Logger"/> class.
    /// </summary>
    public class LogData
    {
        #region 字段

        /// <summary>
        /// Defines the _caller
        /// </summary>
        private StackFrame _caller;

        /// <summary>
        /// Defines the _date
        /// </summary>
        private DateTime _date;

        /// <summary>
        /// Defines the _level
        /// </summary>
        private LogLevel _level;

        /// <summary>
        /// Defines the _message
        /// </summary>
        private string _message;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="LogData"/> class.
        /// </summary>
        /// <param name="level">The level<see cref="LogLevel"/></param>
        /// <param name="caller">The caller<see cref="StackFrame"/></param>
        /// <param name="message">The message<see cref="string"/></param>
        internal LogData(LogLevel level, StackFrame caller, string message)
        {
            _level = level;
            _caller = caller;
            _message = message ?? String.Empty;
            _date = DateTime.Now;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the information of the logging method caller.
        /// </summary>
        public StackFrame Caller
        {
            get
            {
                return _caller;
            }
        }

        /// <summary>
        /// Gets the date and time when the log data was created.
        /// </summary>
        public DateTime Date
        {
            get
            {
                return _date;
            }
        }

        /// <summary>
        /// Gets the logging level of the log data.
        /// </summary>
        public LogLevel Level
        {
            get
            {
                return _level;
            }
        }

        /// <summary>
        /// Gets the message of the log data.
        /// </summary>
        public string Message
        {
            get
            {
                return _message;
            }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="LogData"/>.
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public override string ToString()
        {
            var header = String.Format("{0}|{1,-5}|", _date, _level);
            var method = _caller.GetMethod();
            var type = method.DeclaringType;
#if DEBUG
            var lineNum = _caller.GetFileLineNumber();
            var headerAndCaller =
              String.Format("{0}{1}.{2}:{3}|", header, type.Name, method.Name, lineNum);
#else
      var headerAndCaller = String.Format ("{0}{1}.{2}|", header, type.Name, method.Name);
#endif
            var msgs = _message.Replace("\r\n", "\n").TrimEnd('\n').Split('\n');
            if (msgs.Length <= 1)
                return String.Format("{0}{1}", headerAndCaller, _message);

            var buff = new StringBuilder(String.Format("{0}{1}\n", headerAndCaller, msgs[0]), 64);

            var fmt = String.Format("{{0,{0}}}{{1}}\n", header.Length);
            for (var i = 1; i < msgs.Length; i++)
                buff.AppendFormat(fmt, "", msgs[i]);

            buff.Length--;
            return buff.ToString();
        }

        #endregion 方法
    }
}