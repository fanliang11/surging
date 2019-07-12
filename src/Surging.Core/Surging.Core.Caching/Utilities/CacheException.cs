using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Caching.Utilities
{
    /// <summary>
    /// Defines the <see cref="CacheException" />
    /// </summary>
    internal class CacheException : Exception
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheException"/> class.
        /// </summary>
        public CacheException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheException"/> class.
        /// </summary>
        /// <param name="message">错误信息</param>
        public CacheException(string message)
            : base(message)
        {
            Message = message;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheException"/> class.
        /// </summary>
        /// <param name="message"> 解释异常原因的错误信息。</param>
        /// <param name="e">导致当前异常的异常；如果未指定内部异常，则是一个 null 引用</param>
        public CacheException(string message, Exception e)
            : base(message, e)
        {
            Message = message;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the Message
        /// 错误信息
        /// </summary>
        private new string Message { get; set; }

        #endregion 属性
    }
}