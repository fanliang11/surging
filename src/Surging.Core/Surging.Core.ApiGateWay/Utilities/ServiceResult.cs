using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.ApiGateWay.Utilities
{
    /// <summary>
    /// 自定义错误结果对象
    /// </summary>
    /// <typeparam name="T">需要返回的类型</typeparam>

    public class ServiceResult<T> : ServiceResult
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
        public static ServiceResult<T> Create(bool successd, string message, T entity)
        {
            return new ServiceResult<T>()
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
        public static ServiceResult<T> Create(bool successd, T entity)
        {
            return new ServiceResult<T>()
            {
                IsSucceed = successd,
                Entity = entity
            };
        }
    }

    public class ServiceResult
    {
        /// <summary>
        /// 生成错误信息
        /// </summary>
        /// <param name="message">返回客户端的消息</param>
        /// <returns>返回服务数据集</returns>
        public static ServiceResult Error(string message)
        {
            return new ServiceResult() { Message = message, IsSucceed = false };
        }

        /// <summary>
        /// 生成服务器数据集
        /// </summary>
        /// <param name="success">状态值（true:成功 false：失败）</param>
        /// <param name="successMessage">返回客户端的消息</param>
        /// <param name="errorMessage">错误信息</param>
        /// <returns>返回服务数据集</returns>
        public static ServiceResult Create(bool success, string successMessage = "", string errorMessage = "")
        {
            return new ServiceResult() { Message = success ? successMessage : errorMessage, IsSucceed = success };
        }

        /// <summary>
        /// 构造服务数据集
        /// </summary>
        public ServiceResult()
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
        /// 错误码
        /// </summary>
        public int ErrorCode { get; set; }
    }
}
