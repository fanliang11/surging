using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt.Internal.Enums
{
    public  class ConnectMessage:MqttMessage
    {
        public override MessageType MessageType => MessageType.CONNECT;
        public string ProtocolName { get; set; }

        public int ProtocolLevel { get; set; }

        public bool CleanSession { get; set; }

        public bool HasWill { get; set; }

        public int WillQualityOfService { get; set; }

        public bool WillRetain { get; set; }

        public bool HasPassword { get; set; }

        public bool HasUsername { get; set; }

        public int KeepAliveInSeconds { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string ClientId { get; set; }

        public string WillTopic { get; set; }

        public byte[] WillMessage { get; set; }
    }
}
