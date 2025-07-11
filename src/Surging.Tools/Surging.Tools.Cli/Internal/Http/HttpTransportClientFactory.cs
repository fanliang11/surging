using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Surging.Tools.Cli.Utilities;
using Autofac;
using Surging.Tools.Cli.Commands;

namespace Surging.Tools.Cli.Internal.Http
{
    public class HttpTransportClientFactory : ITransportClientFactory
    {
        private readonly CommandLineApplication _app;
        private readonly IConsole _console;
        private readonly IHttpClientProvider _httpClientProvider;
        public HttpTransportClientFactory(CommandLineApplication app, IConsole console, IHttpClientProvider httpClientProvider)
        {
            _app = app;
            _console = console;
            _httpClientProvider = httpClientProvider;
        }

        public Task<ITransportClient> CreateClientAsync(EndPoint endPoint)
        { 
            return Task.FromResult<ITransportClient>(new HttpTransportClient(_app,_httpClientProvider));
        }
    }
}
