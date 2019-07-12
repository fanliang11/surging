using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.System.Intercept
{
    /// <summary>
    /// 设置判断日志拦截方法的特性类
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface)]
    public class LoggerInterceptAttribute : Attribute
    {
        #region 字段

        /// <summary>
        /// Defines the _message
        /// </summary>
        private string _message;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerInterceptAttribute"/> class.
        /// </summary>
        public LoggerInterceptAttribute()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerInterceptAttribute"/> class.
        /// </summary>
        /// <param name="message">The message<see cref="string"/></param>
        public LoggerInterceptAttribute(string message)
        {
            this._message = message;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the Message
        /// 日志内容
        /// </summary>
        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                _message = value;
            }
        }

        #endregion 属性
    }
}