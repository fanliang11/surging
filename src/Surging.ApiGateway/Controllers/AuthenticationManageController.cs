using Microsoft.AspNetCore.Mvc;
using Surging.Core.ApiGateWay.ServiceDiscovery;
using Surging.Core.ApiGateWay.Utilities;
using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Utilities;
using System.Linq;
using System.Threading.Tasks;

namespace Surging.ApiGateway.Controllers
{
    /// <summary>
    /// Defines the <see cref="AuthenticationManageController" />
    /// </summary>
    public class AuthenticationManageController : Controller
    {
        #region 方法

        /// <summary>
        /// The EditServiceToken
        /// </summary>
        /// <param name="serviceDiscoveryProvider">The serviceDiscoveryProvider<see cref="IServiceDiscoveryProvider"/></param>
        /// <param name="model">The model<see cref="IpAddressModel"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpPost]
        public async Task<IActionResult> EditServiceToken([FromServices]IServiceDiscoveryProvider serviceDiscoveryProvider, IpAddressModel model)
        {
            await serviceDiscoveryProvider.EditServiceToken(model);
            return Json(ServiceResult.Create(true));
        }

        /// <summary>
        /// The EditServiceToken
        /// </summary>
        /// <param name="serviceDiscoveryProvider">The serviceDiscoveryProvider<see cref="IServiceDiscoveryProvider"/></param>
        /// <param name="address">The address<see cref="string"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        public async Task<IActionResult> EditServiceToken([FromServices]IServiceDiscoveryProvider serviceDiscoveryProvider, string address)
        {
            var list = await serviceDiscoveryProvider.GetAddressAsync(address); ;
            return View(list.FirstOrDefault());
        }

        /// <summary>
        /// The Index
        /// </summary>
        /// <returns>The <see cref="IActionResult"/></returns>
        public IActionResult Index()
        {
            return View();
        }

        #endregion 方法
    }
}