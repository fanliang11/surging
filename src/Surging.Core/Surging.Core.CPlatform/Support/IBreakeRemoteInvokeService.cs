using Surging.Core.CPlatform.Messages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Support
{
    public interface IBreakeRemoteInvokeService
    {
        Task<RemoteInvokeResultMessage> InvokeAsync(IDictionary<string, object> parameters, string serviceId, string _serviceKey, bool decodeJOject);
    }
}
