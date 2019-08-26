

using Surging.Core.Protocol.WS;
using Surging.IModuleServices.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surging.Modules.Common.Domain
{
    public class MediaService : WSBehavior, IMediaService
    {
        public   Task Push(IEnumerable<byte> data)
        {
              this.GetClient().Broadcast(data.ToArray());
            return Task.CompletedTask;
        }
    }
}