using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Network
{
    public enum NetworkType
    {
        TcpClient,//TCP客户端
        Tcp,//TCP服务
        MqttClient,//MQTT客户端
        Mqtt,//MQTT服务
        HttpClient,//HTTP客户端
        Http,//HTTP服务
        WSClient,//WebSocket客户端
        WS,//WebSocket服务
        Udp,//UDP
        Grpc,
        CoapClient,//CoAP客户端
        Coap//CoAP服务
    }
}
