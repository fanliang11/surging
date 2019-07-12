using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.EventBusRabbitMQ
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IRabbitMQPersistentConnection" />
    /// </summary>
    public interface IRabbitMQPersistentConnection
         : IDisposable
    {
        #region 事件

        /// <summary>
        /// Defines the OnRabbitConnectionShutdown
        /// </summary>
        event EventHandler<ShutdownEventArgs> OnRabbitConnectionShutdown;

        #endregion 事件

        #region 属性

        /// <summary>
        /// Gets a value indicating whether IsConnected
        /// </summary>
        bool IsConnected { get; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The CreateModel
        /// </summary>
        /// <returns>The <see cref="IModel"/></returns>
        IModel CreateModel();

        /// <summary>
        /// The TryConnect
        /// </summary>
        /// <returns>The <see cref="bool"/></returns>
        bool TryConnect();

        #endregion 方法
    }

    #endregion 接口
}