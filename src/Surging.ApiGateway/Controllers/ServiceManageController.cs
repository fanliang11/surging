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
    /// <summary>
    /// Defines the <see cref="ServiceManageController" />
    /// </summary>
    public class ServiceManageController : Controller
    {
        #region 方法

        /// <summary>
        /// The DelCacheEndPoint
        /// </summary>
        /// <param name="serviceCacheProvider">The serviceCacheProvider<see cref="IServiceCacheProvider"/></param>
        /// <param name="cacheId">The cacheId<see cref="string"/></param>
        /// <param name="endpoint">The endpoint<see cref="string"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpPost]
        public async Task<IActionResult> DelCacheEndPoint([FromServices]IServiceCacheProvider serviceCacheProvider, string cacheId, string endpoint)
        {
            await serviceCacheProvider.DelCacheEndpointAsync(cacheId, endpoint);
            return Json(ServiceResult.Create(true));
        }

        /// <summary>
        /// The EditCacheEndPoint
        /// </summary>
        /// <param name="serviceCacheProvider">The serviceCacheProvider<see cref="IServiceCacheProvider"/></param>
        /// <param name="param">The param<see cref="CacheEndpointParam"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpPost]
        public async Task<IActionResult> EditCacheEndPoint([FromServices]IServiceCacheProvider serviceCacheProvider, CacheEndpointParam param)
        {
            await serviceCacheProvider.SetCacheEndpointByEndpoint(param.CacheId, param.Endpoint, param.CacheEndpoint);
            return Json(ServiceResult.Create(true));
        }

        /// <summary>
        /// The EditCacheEndPoint
        /// </summary>
        /// <param name="serviceCacheProvider">The serviceCacheProvider<see cref="IServiceCacheProvider"/></param>
        /// <param name="cacheId">The cacheId<see cref="string"/></param>
        /// <param name="endpoint">The endpoint<see cref="string"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        public async Task<IActionResult> EditCacheEndPoint([FromServices]IServiceCacheProvider serviceCacheProvider, string cacheId, string endpoint)
        {
            var model = await serviceCacheProvider.GetCacheEndpointAsync(cacheId, endpoint);
            return View(model);
        }

        /// <summary>
        /// The EditFaultTolerant
        /// </summary>
        /// <param name="faultTolerantProvider">The faultTolerantProvider<see cref="IFaultTolerantProvider"/></param>
        /// <param name="model">The model<see cref="ServiceCommandDescriptor"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpPost]
        public async Task<IActionResult> EditFaultTolerant([FromServices]IFaultTolerantProvider faultTolerantProvider, ServiceCommandDescriptor model)
        {
            await faultTolerantProvider.SetCommandDescriptorByAddress(model);
            return Json(ServiceResult.Create(true));
        }

        /// <summary>
        /// The EditFaultTolerant
        /// </summary>
        /// <param name="faultTolerantProvider">The faultTolerantProvider<see cref="IFaultTolerantProvider"/></param>
        /// <param name="serviceId">The serviceId<see cref="string"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        public async Task<IActionResult> EditFaultTolerant([FromServices]IFaultTolerantProvider faultTolerantProvider, string serviceId)
        {
            var list = await faultTolerantProvider.GetCommandDescriptor(serviceId);
            return View(list.FirstOrDefault());
        }

        /// <summary>
        /// The FaultTolerant
        /// </summary>
        /// <param name="serviceId">The serviceId<see cref="string"/></param>
        /// <param name="address">The address<see cref="string"/></param>
        /// <returns>The <see cref="IActionResult"/></returns>
        public IActionResult FaultTolerant(string serviceId, string address)
        {
            ViewBag.ServiceId = serviceId;
            ViewBag.Address = address;
            return View();
        }

        /// <summary>
        /// The GetAddress
        /// </summary>
        /// <param name="serviceDiscoveryProvider">The serviceDiscoveryProvider<see cref="IServiceDiscoveryProvider"/></param>
        /// <param name="queryParam">The queryParam<see cref="string"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpPost]
        public async Task<IActionResult> GetAddress([FromServices]IServiceDiscoveryProvider serviceDiscoveryProvider, string queryParam)
        {
            var list = await serviceDiscoveryProvider.GetAddressAsync(queryParam);
            var result = ServiceResult<IEnumerable<ServiceAddressModel>>.Create(true, list);
            return Json(result);
        }

        /// <summary>
        /// The GetCacheEndpoint
        /// </summary>
        /// <param name="serviceCacheProvider">The serviceCacheProvider<see cref="IServiceCacheProvider"/></param>
        /// <param name="cacheId">The cacheId<see cref="string"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        public async Task<IActionResult> GetCacheEndpoint([FromServices]IServiceCacheProvider serviceCacheProvider,
            string cacheId)
        {
            var list = await serviceCacheProvider.GetCacheEndpointAsync(cacheId);
            var result = ServiceResult<IEnumerable<CacheEndpoint>>.Create(true, list);
            return Json(result);
        }

        /// <summary>
        /// The GetCommandDescriptor
        /// </summary>
        /// <param name="faultTolerantProvider">The faultTolerantProvider<see cref="IFaultTolerantProvider"/></param>
        /// <param name="serviceId">The serviceId<see cref="string"/></param>
        /// <param name="address">The address<see cref="string"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
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

        /// <summary>
        /// The GetRegisterAddress
        /// </summary>
        /// <param name="serviceRegisterProvide">The serviceRegisterProvide<see cref="IServiceRegisterProvider"/></param>
        /// <param name="queryParam">The queryParam<see cref="string"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpPost]
        public async Task<IActionResult> GetRegisterAddress([FromServices]IServiceRegisterProvider serviceRegisterProvide, string queryParam)
        {
            var list = await serviceRegisterProvide.GetAddressAsync(queryParam);
            var result = ServiceResult<IEnumerable<ServiceAddressModel>>.Create(true, list);
            return Json(result);
        }

        /// <summary>
        /// The GetServiceCache
        /// </summary>
        /// <param name="serviceCacheProvider">The serviceCacheProvider<see cref="IServiceCacheProvider"/></param>
        /// <param name="queryParam">The queryParam<see cref="string"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpPost]
        public async Task<IActionResult> GetServiceCache([FromServices]IServiceCacheProvider serviceCacheProvider, string queryParam)
        {
            var list = await serviceCacheProvider.GetServiceDescriptorAsync();
            var result = ServiceResult<IEnumerable<CacheDescriptor>>.Create(true, list);
            return Json(result);
        }

        /// <summary>
        /// The GetServiceDescriptor
        /// </summary>
        /// <param name="serviceDiscoveryProvider">The serviceDiscoveryProvider<see cref="IServiceDiscoveryProvider"/></param>
        /// <param name="address">The address<see cref="string"/></param>
        /// <param name="queryParam">The queryParam<see cref="string"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpPost]
        public async Task<IActionResult> GetServiceDescriptor([FromServices]IServiceDiscoveryProvider serviceDiscoveryProvider, string address, string queryParam)
        {
            var list = await serviceDiscoveryProvider.GetServiceDescriptorAsync(address, queryParam);
            var result = ServiceResult<IEnumerable<ServiceDescriptor>>.Create(true, list);
            return Json(result);
        }

        /// <summary>
        /// The GetSubscriber
        /// </summary>
        /// <param name="serviceSubscribeProvider">The serviceSubscribeProvider<see cref="IServiceSubscribeProvider"/></param>
        /// <param name="queryParam">The queryParam<see cref="string"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpPost]
        public async Task<IActionResult> GetSubscriber([FromServices]IServiceSubscribeProvider serviceSubscribeProvider,
            string queryParam)
        {
            var list = await serviceSubscribeProvider.GetAddressAsync(queryParam);
            var result = ServiceResult<IEnumerable<ServiceAddressModel>>.Create(true, list);
            return Json(result);
        }

        // GET: /<controller>/
        /// <summary>
        /// The Index
        /// </summary>
        /// <returns>The <see cref="IActionResult"/></returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// The ServiceCache
        /// </summary>
        /// <returns>The <see cref="IActionResult"/></returns>
        public IActionResult ServiceCache()
        {
            return View();
        }

        /// <summary>
        /// The ServiceCacheEndpoint
        /// </summary>
        /// <param name="cacheId">The cacheId<see cref="string"/></param>
        /// <returns>The <see cref="IActionResult"/></returns>
        public IActionResult ServiceCacheEndpoint(string cacheId)
        {
            ViewBag.CacheId = cacheId;
            return View();
        }

        /// <summary>
        /// The ServiceDescriptor
        /// </summary>
        /// <param name="address">The address<see cref="string"/></param>
        /// <returns>The <see cref="IActionResult"/></returns>
        public IActionResult ServiceDescriptor(string address)
        {
            ViewBag.address = address;
            return View();
        }

        /// <summary>
        /// The ServiceManage
        /// </summary>
        /// <returns>The <see cref="IActionResult"/></returns>
        public IActionResult ServiceManage()
        {
            return View();
        }

        /// <summary>
        /// The ServiceSubscriber
        /// </summary>
        /// <param name="serviceId">The serviceId<see cref="string"/></param>
        /// <returns>The <see cref="IActionResult"/></returns>
        public IActionResult ServiceSubscriber(string serviceId)
        {
            ViewBag.ServiceId = serviceId;
            return View();
        }

        #endregion 方法
    }
}