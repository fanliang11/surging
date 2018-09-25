using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Http
{
   public enum StatusCode
    {
        Success=200,
        RequestError =400,
        AuthorizationFailed=401,
    }
}
