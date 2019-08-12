using Microsoft.AspNetCore.Http;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Routing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.KestrelHttpServer.Filters.Implementation
{
   public  class ActionExecutingContext
    {
        public HttpMessage Message { get; internal set; }

        public ServiceRoute Route { get; internal set; }

        public HttpResultMessage<object> Result { get; set; }
          
        public HttpContext Context { get; internal set; }
    }
}
