using Surging.Core.CPlatform.Routing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Filters.Implementation
{
   public  class AuthorizationAttribute : AuthorizationFilterAttribute
    {
        public AuthorizationType AuthType { get; set; }
    }
}
