using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Messages
{
    public class HttpResultMessage<T> : HttpResultMessage
    {
        /// <summary>
        /// 数据集
        /// </summary>
        public T Entity { get; set; }

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
    }

    public class HttpResultMessage
    {
        /// <summary>
        /// 生成错误信息
        /// </summary>
        /// <param name="message">返回客户端的消息</param>
        /// <returns>返回服务数据集</returns>
        public static HttpResultMessage Error(string message)
        {
            return new HttpResultMessage() { Message = message, IsSucceed = false };
        }

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
        /// 构造服务数据集
        /// </summary>
        public HttpResultMessage()
        {
            IsSucceed = false;
            Message = string.Empty;
        }

        /// <summary>
        /// 状态值
        /// </summary>

        public bool IsSucceed { get; set; }

        /// <summary>
        ///返回客户端的消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 状态码
        /// </summary>
        public int StatusCode { get; set; }
    }
}
 
