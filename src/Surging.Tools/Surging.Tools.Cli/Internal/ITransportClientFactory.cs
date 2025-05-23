using System.Net;
using System.Threading.Tasks;

namespace Surging.Tools.Cli.Internal
{
    public interface ITransportClientFactory
    {

        Task<ITransportClient> CreateClientAsync(EndPoint endPoint);
    }
}
