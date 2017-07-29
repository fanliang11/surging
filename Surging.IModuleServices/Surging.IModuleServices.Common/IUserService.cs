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

        Task<string> GetUserName(int id);

        Task<bool> Exists(int id);

        Task<int>  GetUserId(string userName);

        Task<DateTime> GetUserLastSignInTime(int id);

        [Command(Strategy= StrategyType.Injection,Injection = @"return
new Surging.IModuleServices.Common.Models.UserModel
         {
            Name=""fanly"",
            Age=18
         };", RequestCacheEnabled =true, InjectionNamespaces = new string[] { "Surging.IModuleServices.Common" })]
        [InterceptMethod(CachingMethod.Get, Key = "GetUser_id_{0}", CacheSectionType =SectionType.ddlCache, Mode = CacheTargetType.Redis, Time = 480)]
        Task<UserModel> GetUser(UserModel user);

        [Command(Strategy = StrategyType.Failover, RequestCacheEnabled = true, InjectionNamespaces = new string[] { "Surging.IModuleServices.Common" })]
        [InterceptMethod(CachingMethod.Remove, "GetUser_id_{0}", "GetUserName_name_{0}", CacheSectionType = SectionType.ddlCache, Mode = CacheTargetType.Redis)]
        Task<bool> Update(int id, UserModel model);

        Task<IDictionary<string, string>> GetDictionary();

     
        Task TryThrowException();

        Task PublishThroughEventBusAsync(IntegrationEvent evt);
    }
}
