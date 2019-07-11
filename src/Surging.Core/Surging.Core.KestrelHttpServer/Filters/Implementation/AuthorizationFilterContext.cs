using Microsoft.AspNetCore.Http;
using Surging.Core.CPlatform.Routing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.KestrelHttpServer.Filters.Implementation
{
   public class AuthorizationFilterContext
    {
        public ServiceRoute Route { get; internal set; }

        public HttpContext Context { get; internal set; }
    }
}
