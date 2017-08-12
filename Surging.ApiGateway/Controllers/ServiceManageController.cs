using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Surging.Core.ProxyGenerator.Utilitys;
using Surging.Core.ApiGateWay.ServiceDiscovery;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Surging.ApiGateway.Controllers
{
    public class ServiceManageController : Controller
    {
        // GET: /<controller>/
        public async Task<IActionResult> Index()
        {
           var list= await ServiceLocator.GetService<IServiceDiscoveryProvider>().GetAddressAsync();
            return View(list);
        }

        public async Task<IActionResult> ServiceDescriptor(string address)
        {
            var list = await ServiceLocator.GetService<IServiceDiscoveryProvider>().GetServiceDescriptorAsync(address);
            return View(list.ToList());
        }
    }
}
