using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Surging.Core.ApiGateWay;
using Surging.Core.CPlatform.Routing;
using Surging.Core.ProxyGenerator;
using Surging.Core.ProxyGenerator.Utilitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surging.ApiGateway.Controllers
{
    public class ServicesController : Controller
    {
        private readonly IServiceProxyProvider _serviceProxyProvider;

        public ServicesController()
        {
            _serviceProxyProvider = ServiceLocator.GetService<IServiceProxyProvider>();
        }
        public async Task<ServiceResult<object>> Path(string path, [FromQuery]string serviceKey, [FromBody]Dictionary<string, object> model)
        {
            ServiceResult<object> result = ServiceResult<object>.Create(false,null);
            if (!string.IsNullOrEmpty(serviceKey))
            {
                result = ServiceResult<object>.Create(true,await _serviceProxyProvider.Invoke<object>(model, path, serviceKey));
            }
            else
            {
                result = ServiceResult<object>.Create(true, await _serviceProxyProvider.Invoke<object>(model, path));
            }
            return result;
        }
    }
}
