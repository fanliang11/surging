using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Tools.Cli.Internal.Netty
{
    public class NettyTransportClientFactory : ITransportClientFactory
    {
        private readonly CommandLineApplication _app;
        private readonly IConsole _console;
        private readonly IHttpClientProvider _httpClientProvider;
        public NettyTransportClientFactory(CommandLineApplication app, IConsole console, IHttpClientProvider httpClientProvider)
        {
            _app = app;
            _console = console;
            _httpClientProvider = httpClientProvider;
        }

        public Task<ITransportClient> CreateClientAsync(EndPoint endPoint)
        {
            return Task.FromResult<ITransportClient>(new NettyTransportClient(_app, _httpClientProvider));
        }
    }
}
