using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Tools.Cli.Internal
{
    public interface ITransportClientFactory
    {

        Task<ITransportClient> CreateClientAsync(EndPoint endPoint);
    }
}
