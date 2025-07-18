using Microsoft.AspNetCore.Http;
using Surging.Core.CPlatform.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protokollwandler.Internal
{
    public interface ITransportClient
    {
        Task<string> SendAsync(string address, IDictionary<string, object> parameters, HttpContext httpContext);
    }
}
