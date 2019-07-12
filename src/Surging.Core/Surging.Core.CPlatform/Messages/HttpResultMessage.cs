using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Messages
{
    /// <summary>
    /// Defines the <see cref="HttpResultMessage{T}" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HttpResultMessage<T> : HttpResultMessage
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Entity
        /// 数据集
        /// </summary>
        public T Entity { get; set; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// 生成自定义服务数据集
        /// </summary>
        /// <param name="successd">状态值（true:成功 false：失败）</param>
        /// <param name="message">返回到客户端的消息</param>
        /// <param name="entity">返回到客户端的数据集</param>
        /// <returns>返回信息结果集</returns>
        public static HttpResultMessage<T> Create(bool successd, string message, T entity)
        {
            return new HttpResultMessage<T>()
            {
                IsSucceed = successd,
                Message = message,
                Entity = entity
            };
        }

        /// <summary>
        /// 生成自定义服务数据集
        /// </summary>
        /// <param name="successd">状态值（true:成功 false:失败）</param>
        /// <param name="entity">返回到客户端的数据集</param>
        /// <returns>返回信息结果集</returns>
        public static HttpResultMessage<T> Create(bool successd, T entity)
        {
            return new HttpResultMessage<T>()
            {
                IsSucceed = successd,
                Entity = entity
            };
        }

        #endregion 方法
    }

    /// <summary>
    /// Defines the <see cref="HttpResultMessage" />
    /// </summary>
    public class HttpResultMessage
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResultMessage"/> class.
        /// </summary>
        public HttpResultMessage()
        {
            IsSucceed = false;
            Message = string.Empty;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets a value indicating whether IsSucceed
        /// 状态值
        /// </summary>
        public bool IsSucceed { get; set; }

        /// <summary>
        /// Gets or sets the Message
        /// 返回客户端的消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the StatusCode
        /// 状态码
        /// </summary>
        public int StatusCode { get; set; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// 生成服务器数据集
        /// </summary>
        /// <param name="success">状态值（true:成功 false：失败）</param>
        /// <param name="successMessage">返回客户端的消息</param>
        /// <param name="errorMessage">错误信息</param>
        /// <returns>返回服务数据集</returns>
        public static HttpResultMessage Create(bool success, string successMessage = "", string errorMessage = "")
        {
            return new HttpResultMessage() { Message = success ? successMessage : errorMessage, IsSucceed = success };
        }

        /// <summary>
        /// 生成错误信息
        /// </summary>
        /// <param name="message">返回客户端的消息</param>
        /// <returns>返回服务数据集</returns>
        public static HttpResultMessage Error(string message)
        {
            return new HttpResultMessage() { Message = message, IsSucceed = false };
        }

        #endregion 方法
    }
}