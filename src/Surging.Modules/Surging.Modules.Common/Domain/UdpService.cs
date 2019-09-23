using Surging.Core.DNS.Runtime;
using Surging.Core.Protocol.Udp.Runtime;
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
        public override async Task<bool> Dispatch(IEnumerable<byte> bytes)
        {
            await this.GetService<IMediaService>().Push(bytes);
            return await Task.FromResult(true);
        }
    }
}
