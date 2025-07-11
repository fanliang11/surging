using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.KestrelHttpServer
{
   public  enum StatusCode
    {
        Success = 200,
        RequestError = 400,
        AuthorizationFailed = 401,
    }
}
