using Surging.Core.CPlatform.Runtime.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt.Internal.Runtime
{
    /// <summary>
    /// Defines the <see cref="MqttRemoteInvokeContext" />
    /// </summary>
    public class MqttRemoteInvokeContext : RemoteInvokeContext
    {
        #region 属性

        /// <summary>
        /// Gets or sets the topic
        /// </summary>
        public string topic { get; set; }

        #endregion 属性
    }
}