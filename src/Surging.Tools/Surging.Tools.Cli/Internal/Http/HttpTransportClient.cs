using Surging.Tools.Cli.Internal.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Tools.Cli.Internal.Http
{
    public class HttpTransportClient : ITransportClient
    {
        public HttpTransportClient()
        {

        }

        public Task<RemoteInvokeResultMessage> SendAsync(RemoteInvokeMessage message, CancellationToken cancellationToken)
        {
            return Task.FromResult(new RemoteInvokeResultMessage() { Result = "http message" });
        }
    }
}
