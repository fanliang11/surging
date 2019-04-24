using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.ApiGateWay
{
   public enum ServiceStatusCode
    {
        Success=200,
        RequestError =400,
        AuthorizationFailed=401,
    }
}
