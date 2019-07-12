using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.EventBusKafka
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IKafkaPersisterConnection" />
    /// </summary>
    public interface IKafkaPersisterConnection : IDisposable
    {
        #region 属性

        /// <summary>
        /// Gets a value indicating whether IsConnected
        /// </summary>
        bool IsConnected { get; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The CreateConnect
        /// </summary>
        /// <returns>The <see cref="Object"/></returns>
        Object CreateConnect();

        /// <summary>
        /// The TryConnect
        /// </summary>
        /// <returns>The <see cref="bool"/></returns>
        bool TryConnect();

        #endregion 方法
    }

    #endregion 接口
}