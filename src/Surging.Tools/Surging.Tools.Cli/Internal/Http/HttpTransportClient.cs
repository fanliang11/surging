using McMaster.Extensions.CommandLineUtils;
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
        private readonly CommandLineApplication _app;
        public HttpTransportClient(CommandLineApplication app)
        {
            _app = app;
        }

        public Task<RemoteInvokeResultMessage> SendAsync( CancellationToken cancellationToken)
        {
            return Task.FromResult(new RemoteInvokeResultMessage() { Result = $"http message, http  Arguments 'data' :{_app.Arguments[0].Value}" });
        }
    }
}
