using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Surging.Core.ProxyGenerator.Utilitys;
using Surging.Core.ApiGateWay.ServiceDiscovery;
using Surging.Core.ApiGateWay.Utilities;
using Surging.Core.ApiGateWay.ServiceDiscovery.Implementation;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Support;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Surging.ApiGateway.Controllers
{
    public class ServiceManageController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetAddress(string queryParam)
        {
            var list = await ServiceLocator.GetService<IServiceDiscoveryProvider>().GetAddressAsync(queryParam);
            var result = ServiceResult<IEnumerable<ServiceAddressModel>>.Create(true, list);
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> GetServiceDescriptor(string address,string queryParam)
        {
            var list = await ServiceLocator.GetService<IServiceDiscoveryProvider>().GetServiceDescriptorAsync(address,queryParam);
            var result = ServiceResult<IEnumerable<ServiceDescriptor>>.Create(true, list);
            return Json(result);
        }

        public IActionResult ServiceDescriptor(string address)
        {
            ViewBag.address = address;
            return View();
        }

        public IActionResult FaultTolerant(params string [] serviceIds)
        {
            ViewBag.ServiceIds = $" ['{string.Join(",", serviceIds)}']";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetCommandDescriptor(string[] serviceIds)
        {
            var list = await ServiceLocator.GetService<IFaultTolerantProvider>().GetCommandDescriptor(serviceIds);
            var result = ServiceResult<IEnumerable<ServiceCommandDescriptor>>.Create(true, list);
            return Json(result);
        }
    }
}
