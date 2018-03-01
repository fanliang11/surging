using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.ProxyGenerator;

namespace GateWay.WebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/BaseApi")]
    public class BaseApiController : Controller //ControllerBase
    {
        protected IServiceProxyFactory serviceProxyFactory = ServiceLocator.GetService<IServiceProxyFactory>();
        protected readonly IDistributedCache _cache;
    }
}