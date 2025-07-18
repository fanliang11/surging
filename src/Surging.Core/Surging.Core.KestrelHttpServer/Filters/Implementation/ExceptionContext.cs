using Microsoft.AspNetCore.Http;
using Surging.Core.CPlatform.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.KestrelHttpServer.Filters.Implementation
{
   public class ExceptionContext
    {
        public string RoutePath { get;  set; }

        public Exception Exception { get;  set; }

        public HttpResultMessage<object> Result { get; set; }

        public HttpContext Context { get;  set; }
    }
}
