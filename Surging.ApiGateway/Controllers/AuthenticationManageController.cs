using Microsoft.AspNetCore.Mvc;
using Surging.Core.ApiGateWay.ServiceDiscovery;
using Surging.Core.ApiGateWay.Utilities;
using Surging.Core.ProxyGenerator.Utilitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surging.ApiGateway.Controllers
{
    public class AuthenticationManageController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> EditServiceToken(string address)
        {
            var list = await ServiceLocator.GetService<IServiceDiscoveryProvider>().GetAddressAsync(address); ;
            return View(list.FirstOrDefault());
        }
    }
}
