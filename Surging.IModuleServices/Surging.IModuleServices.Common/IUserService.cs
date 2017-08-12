using Surging.Core.Caching;
using Surging.Core.CPlatform.EventBus.Events;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Surging.Core.CPlatform.Support;
using Surging.Core.CPlatform.Support.Attributes;
using Surging.Core.ProxyGenerator.Implementation;
using Surging.Core.System.Intercept;
using Surging.IModuleServices.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.IModuleServices.Common
{
    [ServiceBundle]
    public interface IUserService
    {

        [Service(Date ="2017-8-11",Director ="fanly",Name ="获取用户")]
        Task<string> GetUserName(int id);

        [Service(Date = "2017-8-11", Director = "fanly", Name = "根据id查找用户是否存在")]
        Task<bool> Exists(int id);

        [Service(Date = "2017-8-11", Director = "fanly", Name = "获取用户")]
        Task<int>  GetUserId(string userName);

        [Service(Date = "2017-8-11", Director = "fanly", Name = "获取用户")]
        Task<DateTime> GetUserLastSignInTime(int id);

        [Command(Strategy= StrategyType.Injection,Injection = @"return
new Surging.IModuleServices.Common.Models.UserModel
         {
            Name=""fanly"",
            Age=19
         };", RequestCacheEnabled =false, InjectionNamespaces = new string[] { "Surging.IModuleServices.Common" })]
        [Service(Date = "2017-8-11", Director = "fanly", Name = "获取用户")]
        [InterceptMethod(CachingMethod.Get, Key = "GetUser_id_{0}", CacheSectionType =SectionType.ddlCache, Mode = CacheTargetType.Redis, Time = 480)]
        Task<UserModel> GetUser(UserModel user);

        [Command(Strategy = StrategyType.Failover, RequestCacheEnabled = true, InjectionNamespaces = new string[] { "Surging.IModuleServices.Common" })]
        [InterceptMethod(CachingMethod.Remove, "GetUser_id_{0}", "GetUserName_name_{0}", CacheSectionType = SectionType.ddlCache, Mode = CacheTargetType.Redis)]
        [Service(Date = "2017-8-11", Director = "fanly", Name = "获取用户")]
        Task<bool> Update(int id, UserModel model);

        [Service(Date = "2017-8-11", Director = "fanly", Name = "获取用户")]
        Task<IDictionary<string, string>> GetDictionary();

        [Service(Date = "2017-8-11", Director = "fanly", Name = "获取用户")]
        Task TryThrowException();

        [Service(Date = "2017-8-11", Director = "fanly", Name = "获取用户")]
        Task PublishThroughEventBusAsync(IntegrationEvent evt);
    }
}
