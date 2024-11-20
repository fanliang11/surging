using DotNetty.Buffers;
using Surging.Core.Protocol.Tcp.Runtime;
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

        public override void Load(string clientId, TcpServerProperties tcpServerProperties)
        {
            var deviceStatus = _deviceProvider.IsConnected(clientId);
            this.Parser.HandlePayload().Subscribe(buffer => ParserBuffer(buffer));
        }

        public override void DeviceStatusProcess(DeviceStatus status, string clientId, TcpServerProperties tcpServerProperties)
        {
            //throw new NotImplementedException();
        }

        public async Task ParserBuffer(IByteBuffer buffer)
        {
            List<string> result = new List<string>();
            while (buffer.ReadableBytes > 0)
            {
                result.Add(buffer.ReadString(this.Parser.GetNextFixedRecordLength(),
                    Encoding.GetEncoding("gb2312")));
            }

            //  var str= buffer.ReadString(buffer.ReadableBytes, Encoding.GetEncoding("gb2312"));

            var byteBuffer=  Unpooled.Buffer();
            byteBuffer.WriteString("\r\n", Encoding.UTF8); 
            byteBuffer.WriteString("processing complete", Encoding.UTF8);
            await Sender.SendAndFlushAsync(byteBuffer);
            buffer.Release();
            //  await Sender.SendAndFlushAsync("message received",Encoding.UTF8);
            this.Parser.Close(); 
        }

    }
}
