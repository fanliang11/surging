using Microsoft.AspNetCore.Http;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Transport;
using Surging.Core.KestrelHttpServer.Runtime;
using Surging.Core.ProxyGenerator.Interceptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.KestrelHttpServer.Interceptors.Implementation
{
    public class HttpInvocation : IHttpInvocation
    {
        public IHttpMessageSender Sender { get; internal set; }
        public HttpServiceEntry Entry { get;internal  set; }
        public string Path { get; internal set; }

        public HttpResultMessage<object> Result { get; set; }

        public HttpContext Context { get; internal set; }

        public string NetworkId { get; internal set; }

        public virtual async Task<bool> Proceed()
        {
            var httpBehavior = Entry.Behavior();
            httpBehavior.Sender = Sender;
            httpBehavior.NetworkId.OnNext(NetworkId);
            return  await httpBehavior.CallInvoke(this);
        }
    }
}
