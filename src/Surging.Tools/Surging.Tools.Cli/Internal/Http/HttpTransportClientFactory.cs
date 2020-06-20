using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Surging.Tools.Cli.Internal.Http
{
    public class HttpTransportClientFactory : ITransportClientFactory
    {
        private readonly CommandLineApplication _app;
        public HttpTransportClientFactory(IServiceProvider serviceProvider)
        {
            _app = serviceProvider.GetService<CommandLineApplication>();
        }

        public Task<ITransportClient> CreateClientAsync(EndPoint endPoint)
        {
            return Task.FromResult<ITransportClient>(new HttpTransportClient());
        }
    }
}
