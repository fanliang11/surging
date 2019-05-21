using Surging.Core.CPlatform.Runtime.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Mqtt.Internal.Runtime
{
    public interface IMqttRemoteInvokeService
    {
      
        Task InvokeAsync(RemoteInvokeContext context);
        
        Task InvokeAsync(RemoteInvokeContext context, CancellationToken cancellationToken);
        
        Task InvokeAsync(RemoteInvokeContext context, int requestTimeout);
    }
}
