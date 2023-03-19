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
        TcpServer,//TCP服务
        MqttClient,//MQTT客户端
        MqttServer,//MQTT服务
        HttpClient,//HTTP客户端
        HttpServer,//HTTP服务
        WebSocketClient,//WebSocket客户端
        WebSocketServer,//WebSocket服务
        UDP,//UDP
        CoapClient,//CoAP客户端
        CoapServer//CoAP服务
    }
}
