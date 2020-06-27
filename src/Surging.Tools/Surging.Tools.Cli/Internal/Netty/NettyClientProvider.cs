using McMaster.Extensions.CommandLineUtils;
using Surging.Tools.Cli.Commands;
using Surging.Tools.Cli.Internal.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Tools.Cli.Internal.Netty
{
    public class NettyTransportClient : ITransportClient
    {
        private readonly CommandLineApplication<CurlCommand> _app;
        private readonly IHttpClientProvider _httpClientProvider;
        public NettyTransportClient(CommandLineApplication app, IHttpClientProvider httpClientProvider)
        {
            _app = app as CommandLineApplication<CurlCommand>;
            _httpClientProvider = httpClientProvider;
        }

        public Task<RemoteInvokeResultMessage> SendAsync(CancellationToken cancellationToken)
        {
            var command = _app.Model;
            return Task.FromResult(new RemoteInvokeResultMessage() { Result = $"netty message, http  Arguments 'address' :{command.Address}" });
        }
    }
}
