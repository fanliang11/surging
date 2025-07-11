using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Device.Implementation.Http
{
    public enum HttpStatus
    {
        Success = 200,
        RequestError = 400,
        AuthorizationFailed = 401,
        Forbidden = 403,
        NotFound = 403,
        MethodNotAllowed = 405,
        RequestTimeout=408,
        UnsupportedMediaType= 415,
        ServerError=500,
        NotImplemented=501,
       ServiceUnavailable=503, 

    }
}
