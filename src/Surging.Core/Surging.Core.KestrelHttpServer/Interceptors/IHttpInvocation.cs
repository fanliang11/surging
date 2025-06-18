using Microsoft.AspNetCore.Http;
using Surging.Core.CPlatform.Messages;
using Surging.Core.KestrelHttpServer.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.KestrelHttpServer.Interceptors
{
    public interface IHttpInvocation
    {
          Task<bool> Proceed();

          public string NetworkId { get; }

          HttpServiceEntry Entry { get;  }
          string Path { get;}

          HttpResultMessage<object> Result { get; set; }

          HttpContext Context { get; }
    }
}
