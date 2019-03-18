using Microsoft.AspNetCore.Mvc;
using Surging.ApiGateway.Models;
using Surging.Core.ApiGateWay.ServiceDiscovery;
using Surging.Core.ApiGateWay.ServiceDiscovery.Implementation;
using Surging.Core.ApiGateWay.Utilities;
using Surging.Core.Caching.HashAlgorithms;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Cache;
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

        public IActionResult ServiceManage()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> GetRegisterAddress([FromServices]IServiceRegisterProvider serviceRegisterProvide, string queryParam)
        {
            var list = await serviceRegisterProvide.GetAddressAsync(queryParam);
            var result = ServiceResult<IEnumerable<ServiceAddressModel>>.Create(true, list);
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> GetAddress([FromServices]IServiceDiscoveryProvider serviceDiscoveryProvider, string queryParam)
        {
            var list = await serviceDiscoveryProvider.GetAddressAsync(queryParam);
            var result = ServiceResult<IEnumerable<ServiceAddressModel>>.Create(true, list);
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> GetServiceDescriptor([FromServices]IServiceDiscoveryProvider serviceDiscoveryProvider, string address, string queryParam)
        {
            var list = await serviceDiscoveryProvider.GetServiceDescriptorAsync(address, queryParam);
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

        public async Task<IActionResult> EditCacheEndPoint([FromServices]IServiceCacheProvider serviceCacheProvider, string cacheId,string endpoint)
        {
            var model = await serviceCacheProvider.GetCacheEndpointAsync(cacheId, endpoint);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DelCacheEndPoint([FromServices]IServiceCacheProvider serviceCacheProvider, string cacheId, string endpoint)
        {
            await serviceCacheProvider.DelCacheEndpointAsync(cacheId, endpoint);
            return Json(ServiceResult.Create(true));
        }

        [HttpPost]
        public async Task<IActionResult> EditCacheEndPoint([FromServices]IServiceCacheProvider serviceCacheProvider, CacheEndpointParam param)
        {
          await serviceCacheProvider.SetCacheEndpointByEndpoint(param.CacheId, param.Endpoint, param.CacheEndpoint);
            return Json(ServiceResult.Create(true));
        }

        public async Task<IActionResult> EditFaultTolerant([FromServices]IFaultTolerantProvider faultTolerantProvider,string serviceId)
        {
           var  list = await faultTolerantProvider.GetCommandDescriptor(serviceId);
            return View(list.FirstOrDefault());
        }
        
        [HttpPost]
        public async Task<IActionResult> EditFaultTolerant([FromServices]IFaultTolerantProvider faultTolerantProvider, ServiceCommandDescriptor model)
        {
              await faultTolerantProvider.SetCommandDescriptorByAddress(model);
            return Json(ServiceResult.Create(true));
        }

        [HttpPost]
        public async Task<IActionResult> GetCommandDescriptor([FromServices]IFaultTolerantProvider faultTolerantProvider, 
            string serviceId, string address)
        {
            IEnumerable<ServiceCommandDescriptor> list = null;
            if (!string.IsNullOrEmpty(serviceId))
            {
                list = await faultTolerantProvider.GetCommandDescriptor(serviceId);
            }
            else if (!string.IsNullOrEmpty(address))
            {
                list = await faultTolerantProvider.GetCommandDescriptorByAddress(address);
            }
            var result = ServiceResult<IEnumerable<ServiceCommandDescriptor>>.Create(true, list);
            return Json(result);
        }

        public IActionResult ServiceCache()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetServiceCache([FromServices]IServiceCacheProvider serviceCacheProvider, string queryParam)
        {
            var list = await serviceCacheProvider.GetServiceDescriptorAsync();
            var result = ServiceResult<IEnumerable<CacheDescriptor>>.Create(true, list);
            return Json(result);
        }

        public  IActionResult ServiceCacheEndpoint(string cacheId)
        {
            ViewBag.CacheId = cacheId;
            return View();
        }

        public async Task<IActionResult> GetCacheEndpoint([FromServices]IServiceCacheProvider serviceCacheProvider, 
            string cacheId)
        {
            var list = await serviceCacheProvider.GetCacheEndpointAsync(cacheId);
            var result = ServiceResult<IEnumerable<CacheEndpoint>>.Create(true, list);
            return Json(result);
        }


        public IActionResult ServiceSubscriber(string serviceId)
        {
            ViewBag.ServiceId = serviceId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetSubscriber([FromServices]IServiceSubscribeProvider serviceSubscribeProvider, 
            string queryParam)
        {
            var list = await serviceSubscribeProvider.GetAddressAsync(queryParam);
            var result = ServiceResult<IEnumerable<ServiceAddressModel>>.Create(true, list);
            return Json(result);
        }
    }
}
