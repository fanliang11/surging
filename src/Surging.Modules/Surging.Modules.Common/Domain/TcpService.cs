using DotNetty.Buffers;
using Surging.Core.CPlatform.Network;
using Surging.Core.Protocol.Tcp.Runtime;
using Surging.Core.Protocol.Tcp.Runtime.Implementation;
using Surging.Core.Protocol.Udp.Messages;
using Surging.IModuleServices.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Modules.Common.Domain
{
    public class TcpService : TcpBehavior, ITcpService
    { 
        private readonly IDeviceProvider _deviceProvider;
        public TcpService(IDeviceProvider deviceProvider)
        {
            _deviceProvider = deviceProvider;
        }
         

        

        public async Task ParserBuffer(IByteBuffer buffer)
        {
            List<string> result = new List<string>();
            while (buffer.ReadableBytes > 0)
            {
                result.Add(buffer.ReadString(this.Parser.GetNextFixedRecordLength(),
                    Encoding.GetEncoding("gb2312")));
            }

            // var str= buffer.ReadString(buffer.ReadableBytes, Encoding.UTF8);
            
            var byteBuffer=  Unpooled.Buffer();
            byteBuffer.WriteString("\r\n", Encoding.UTF8); 
            byteBuffer.WriteString("处理完成", Encoding.GetEncoding("gb2312"));
            await Sender.SendAndFlushAsync(byteBuffer);
            buffer.Release();
            //  await Sender.SendAndFlushAsync("消息已接收",Encoding.GetEncoding("gb2312"));
            this.Parser.Close(); 
        }

        public override void Load(TcpClient client, NetworkProperties tcpServerProperties)
        { 
            this.Parser.HandlePayload().Subscribe(async buffer => await ParserBuffer(buffer));
        }

        public override void DeviceStatusProcess(DeviceStatus status, string clientId, NetworkProperties tcpServerProperties)
        {
        }
    }
}
