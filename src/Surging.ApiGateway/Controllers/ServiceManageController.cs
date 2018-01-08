using Microsoft.AspNetCore.Mvc;
using Surging.Core.ApiGateWay.ServiceDiscovery;
using Surging.Core.ApiGateWay.ServiceDiscovery.Implementation;
using Surging.Core.ApiGateWay.Utilities;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Support;
using Surging.Core.CPlatform.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public async Task<IActionResult> GetServiceDescriptor(string address, string queryParam)
        {
            var list = await ServiceLocator.GetService<IServiceDiscoveryProvider>().GetServiceDescriptorAsync(address, queryParam);
            var result = ServiceResult<IEnumerable<ServiceDescriptor>>.Create(true, list);
            return Json(result);
        }

        public IActionResult ServiceDescriptor(string address)
        {
            ViewBag.address = address;
            return View();
        }

        public IActionResult FaultTolerant(string serviceId, string address)
        {
            ViewBag.ServiceId = serviceId;
            ViewBag.Address = address;
            return View();
        }

        public async Task<IActionResult> EditFaultTolerant(string serviceId)
        {
           var  list = await ServiceLocator.GetService<IFaultTolerantProvider>().GetCommandDescriptor(serviceId);
            return View(list.FirstOrDefault());
        }

        [HttpPost]
        public async Task<IActionResult> EditFaultTolerant(ServiceCommandDescriptor model)
        {
              await ServiceLocator.GetService<IFaultTolerantProvider>().SetCommandDescriptorByAddress(model);
            return Json(ServiceResult.Create(true));
        }

        [HttpPost]
        public async Task<IActionResult> GetCommandDescriptor(string serviceId, string address)
        {
            IEnumerable<ServiceCommandDescriptor> list = null;
            if (!string.IsNullOrEmpty(serviceId))
            {
                list = await ServiceLocator.GetService<IFaultTolerantProvider>().GetCommandDescriptor(serviceId);
            }
            else if (!string.IsNullOrEmpty(address))
            {
                list = await ServiceLocator.GetService<IFaultTolerantProvider>().GetCommandDescriptorByAddress(address);
            }
            var result = ServiceResult<IEnumerable<ServiceCommandDescriptor>>.Create(true, list);
            return Json(result);
        }

        public IActionResult ServiceSubscriber(string serviceId)
        {
            ViewBag.ServiceId = serviceId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetSubscriber(string queryParam)
        {
            var list = await ServiceLocator.GetService<IServiceSubscribeProvider>().GetAddressAsync(queryParam);
            var result = ServiceResult<IEnumerable<ServiceAddressModel>>.Create(true, list);
            return Json(result);
        }
    }
}
