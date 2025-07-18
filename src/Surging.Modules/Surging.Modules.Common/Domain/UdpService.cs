using DotNetty.Buffers;
using Surging.Core.CPlatform.Codecs.Message;
using Surging.Core.CPlatform.Network;
using Surging.Core.DNS.Runtime;
using Surging.Core.Protocol.Udp.Messages;
using Surging.Core.Protocol.Udp.Runtime;
using Surging.Core.Protocol.Udp.Runtime.Implementation;
using Surging.IModuleServices.Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Modules.Common.Domain
{
    public class UdpService : UdpBehavior,IUdpService
    {
        public override Task Dispatch(IEnumerable<byte> bytes)
        {
            return Task.CompletedTask;
        }

        public override void Load(UdpClient client, NetworkProperties udpServerProperties)
        {
            client.SendMessage(new UdpMessage(Unpooled.CopiedBuffer(Encoding.UTF8.GetBytes("hello,world"))));
        }
    }
}
