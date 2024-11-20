using Microsoft.AspNetCore.Http;
using Surging.Core.CPlatform.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.KestrelHttpServer.Runtime
{
    public class HttpServiceContext
    {
        public HttpServiceContext() { }
        public HttpServiceContext(string path, HttpContext context) { Path = path; Context = context; }
        public string Path { get; internal set; }

        public HttpResultMessage<object> Result { get; set; }

        public HttpContext Context { get; internal set; }
    }
}
