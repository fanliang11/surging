using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Surging.Core.CPlatform.Routing;
using Surging.Core.ProxyGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surging.ApiGateway.Controllers
{
    public class ServicesController : Controller
    {
        private readonly IServiceProxyProvider _serviceProxyProvider;
        public ServicesController(IServiceProxyProvider serviceProxyProvider)
        {
            _serviceProxyProvider = serviceProxyProvider;
        }
        public async Task<string> Path(string path, [FromQuery]string serviceKey, [FromBody]Dictionary<string, object> model)
        {
            string result = "";
            if (!string.IsNullOrEmpty(serviceKey))
            {
                result = await _serviceProxyProvider.Invoke<string>(model, path, serviceKey);
            }
            else
            {
                result = await _serviceProxyProvider.Invoke<string>(model, path);
            }
            return result;
        }
    }
}
