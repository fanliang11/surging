using DotNetty.Codecs.Mqtt.Packets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt.Internal.Enums
{
    /// <summary>
    /// Defines the <see cref="MqttMessage" />
    /// </summary>
    public abstract class MqttMessage
    {
        #region 属性

        /// <summary>
        /// Gets or sets a value indicating whether Duplicate
        /// </summary>
        public virtual bool Duplicate { get; set; }

        /// <summary>
        /// Gets the MessageType
        /// </summary>
        public abstract MessageType MessageType { get; }

        /// <summary>
        /// Gets or sets the Qos
        /// </summary>
        public virtual int Qos { get; set; } = (int)QualityOfService.AtMostOnce;

        /// <summary>
        /// Gets or sets a value indicating whether RetainRequested
        /// </summary>
        public virtual bool RetainRequested { get; set; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The ToString
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public override string ToString()
        {
            return $"{this.GetType().Name}[Qos={this.Qos}, Duplicate={this.Duplicate}, Retain={this.RetainRequested}]";
        }

        #endregion 方法
    }
}