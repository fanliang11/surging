using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt.Internal.Enums
{
    /// <summary>
    /// Defines the <see cref="ConnectMessage" />
    /// </summary>
    public class ConnectMessage : MqttMessage
    {
        #region 属性

        /// <summary>
        /// Gets or sets a value indicating whether CleanSession
        /// </summary>
        public bool CleanSession { get; set; }

        /// <summary>
        /// Gets or sets the ClientId
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether HasPassword
        /// </summary>
        public bool HasPassword { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether HasUsername
        /// </summary>
        public bool HasUsername { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether HasWill
        /// </summary>
        public bool HasWill { get; set; }

        /// <summary>
        /// Gets or sets the KeepAliveInSeconds
        /// </summary>
        public int KeepAliveInSeconds { get; set; }

        /// <summary>
        /// Gets the MessageType
        /// </summary>
        public override MessageType MessageType => MessageType.CONNECT;

        /// <summary>
        /// Gets or sets the Password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the ProtocolLevel
        /// </summary>
        public int ProtocolLevel { get; set; }

        /// <summary>
        /// Gets or sets the ProtocolName
        /// </summary>
        public string ProtocolName { get; set; }

        /// <summary>
        /// Gets or sets the Username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the WillMessage
        /// </summary>
        public byte[] WillMessage { get; set; }

        /// <summary>
        /// Gets or sets the WillQualityOfService
        /// </summary>
        public int WillQualityOfService { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether WillRetain
        /// </summary>
        public bool WillRetain { get; set; }

        /// <summary>
        /// Gets or sets the WillTopic
        /// </summary>
        public string WillTopic { get; set; }

        #endregion 属性
    }
}