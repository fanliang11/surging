using Microsoft.AspNetCore.Http;
using SoapCore.Extensibility;
using SoapCore.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.WebService.Runtime.Implementation
{
    public class ServiceOperationTuner : IServiceOperationTuner
    {
        public void Tune(HttpContext httpContext, object serviceInstance, OperationDescription operation)
        {
            var service = serviceInstance as WebServiceBehavior;
            service?.ParseHeaderFromBody(httpContext.Request.Body);
        }
    }
}
