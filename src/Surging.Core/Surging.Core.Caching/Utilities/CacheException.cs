using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Caching.Utilities
{
    internal class CacheException : Exception
    {
        /// <summary>
        /// 初始化 System.Exception 类的新实例。
        /// </summary>
        public CacheException()
        {
        }

        /// <summary>
        /// 使用指定的错误信息初始化 System.Exception 类的新实例。
        /// </summary>
        /// <param name="message">错误信息</param>
        public CacheException(string message)
            : base(message)
        {
            Message = message;
        }

        /// <summary>
        /// 使用指定错误消息和对作为此异常原因的内部异常的引用来初始化 System.Exception 类的新实例。
        /// </summary>
        /// <param name="message"> 解释异常原因的错误信息。</param>
        /// <param name="e">导致当前异常的异常；如果未指定内部异常，则是一个 null 引用</param>
        public CacheException(string message, Exception e)
            : base(message, e)
        {
            Message = message;
        }

        /// <summary>
        /// 错误信息
        /// </summary>
        private new string Message
        {
            get;
            set;
        }
    }
}
