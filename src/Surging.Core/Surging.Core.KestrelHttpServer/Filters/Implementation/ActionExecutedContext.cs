using Microsoft.AspNetCore.Http;
using Surging.Core.CPlatform.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.KestrelHttpServer.Filters.Implementation
{
   public class ActionExecutedContext
    {
        public HttpMessage Message { get; internal set; }
        public HttpContext Context { get; internal set; }
    }
}
