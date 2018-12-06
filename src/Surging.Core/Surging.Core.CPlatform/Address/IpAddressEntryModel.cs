using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Address
{
   public sealed class IpAddressEntryModel
    {
        #region Constructor
        
        public IpAddressEntryModel()
        {
        }
        
        public IpAddressEntryModel(string ip, int port,int wsPort,int mqttPort,int httpPort)
        {
            Ip = ip;
            Port = port;
            WsPort = wsPort;
            MqttPort = mqttPort;
            HttpPort = httpPort; 
        }

        #endregion Constructor

        #region Property

        /// <summary>
        /// ip地址。
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// 端口。
        /// </summary>
        public int Port { get; set; }

        public int WsPort { get; set; }

        public int MqttPort { get; set; }

        public int HttpPort { get; set; }

        #endregion Property
    }
}
