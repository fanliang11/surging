using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.ProxyGenerator;

namespace Centa.Agency.WebApi.Controllers
{
    /// <summary>
    /// 身份认证
    /// </summary>
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        [HttpPost("token")]
        public object SignIn( [FromBody]Dictionary<string, object> model)
        {
            model.Add("user", JsonConvert.SerializeObject(new
            {
                Name = "fanly",
                Age = 18,
                UserId = 1
            }));
            string path = "api/AuthApp/SignIn";
            string serviceKey = "Auth";


            var serviceProxyProvider = ServiceLocator.GetService<IServiceProxyProvider>();
            return serviceProxyProvider.Invoke<object>(model, path, serviceKey).Result;
         
        }
    }
}
