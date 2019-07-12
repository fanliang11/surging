using Surging.Core.Common.Extensions;
using System;

namespace Surging.Core.Common.ServicesException
{
    /// <summary>
    /// Defines the <see cref="ServiceException" />
    /// </summary>
    public sealed class ServiceException : Exception
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceException"/> class.
        /// </summary>
        public ServiceException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceException"/> class.
        /// </summary>
        /// <param name="sysCode">错误号</param>
        public ServiceException(Enum sysCode)
                : base(sysCode.GetDisplay())
        {
            this.Code = (int)Enum.Parse(sysCode.GetType(), sysCode.ToString());
            this.Message = sysCode.GetDisplay();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceException"/> class.
        /// </summary>
        /// <param name="sysCode">The sysCode<see cref="Enum"/></param>
        /// <param name="message">The message<see cref="string"/></param>
        public ServiceException(Enum sysCode, string message)
        {
            this.Code = (int)Enum.Parse(sysCode.GetType(), sysCode.ToString());
            this.Message = string.Format(message, sysCode.GetDisplay());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceException"/> class.
        /// </summary>
        /// <param name="message">错误信息</param>
        public ServiceException(string message)
                : base(message)
        {
            Message = message;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceException"/> class.
        /// </summary>
        /// <param name="message"> 解释异常原因的错误信息。</param>
        /// <param name="e">导致当前异常的异常；如果未指定内部异常，则是一个 null 引用</param>
        public ServiceException(string message, Exception e)
                : base(message, e)
        {
            Message = string.IsNullOrEmpty(message) ? e.Message : message;
            this.Source = e.Source;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the Code
        /// 错误号
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// Gets or sets the Message
        /// 错误信息
        /// </summary>
        public new string Message { get; set; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// 通过自定义错误枚举对象获取ServiceException
        /// </summary>
        /// <typeparam name="T">自定义错误枚举</typeparam>
        /// <returns>返回ServiceException</returns>
        public ServiceException GetServiceException<T>()
        {
            var code = Message.Substring(Message.LastIndexOf("错误号", System.StringComparison.Ordinal) + 3);
            var sysCode = Enum.Parse(typeof(T), code);
            this.Code = (int)Enum.Parse(sysCode.GetType(), sysCode.ToString());

            return this;
        }

        #endregion 方法
    }
}