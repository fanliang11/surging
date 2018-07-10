using Newtonsoft.Json.Linq;
using Surging.Core.Caching;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.EventBus.Events;
using Surging.Core.CPlatform.Filters.Implementation;
using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Routing.Implementation;
using Surging.Core.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors.Implementation;
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
    [ServiceBundle("api/{Service}")]
    public interface IUserService: IServiceKey
    {
        Task<UserModel> Authentication(AuthenticationRequestData requestData);
        
        Task<string> GetUserName(int id);

      
        Task<bool> Exists(int id);

        [Authorization(AuthType = AuthorizationType.JWT)]
        Task<IdentityUser> Save(IdentityUser requestData);

        [Authorization(AuthType = AuthorizationType.JWT)]
        [Command(Strategy = StrategyType.Injection, ShuntStrategy = AddressSelectorMode.HashAlgorithm, ExecutionTimeoutInMilliseconds = 1500, BreakerRequestVolumeThreshold = 3, Injection = @"return 1;", RequestCacheEnabled = false)]
        Task<int> GetUserId(string userName);
        
        Task<DateTime> GetUserLastSignInTime(int id);

        [Command(Strategy = StrategyType.Injection, Injection = @"return
new Surging.IModuleServices.Common.Models.UserModel
         {
            Name=""fanly"",
            Age=19
         };", RequestCacheEnabled = false, InjectionNamespaces = new string[] { "Surging.IModuleServices.Common" })]
        [InterceptMethod(CachingMethod.Get, Key = "GetUser_id_{0}", CacheSectionType = SectionType.ddlCache, Mode = CacheTargetType.Redis, Time = 480)]
        Task<UserModel> GetUser(UserModel user);

        [Authorization(AuthType = AuthorizationType.JWT)]
        [Command(Strategy = StrategyType.FallBack,FallBackName = "UpdateFallBackName",  RequestCacheEnabled = true, InjectionNamespaces = new string[] { "Surging.IModuleServices.Common" })]
        [InterceptMethod(CachingMethod.Remove, "GetUser_id_{0}", "GetUserName_name_{0}", CacheSectionType = SectionType.ddlCache, Mode = CacheTargetType.Redis)]
        Task<bool> Update(int id, UserModel model);

        Task<bool> Get(List<UserModel> users);

        
        [Command(Strategy = StrategyType.Injection,ShuntStrategy = AddressSelectorMode.Polling, ExecutionTimeoutInMilliseconds = 1500, BreakerRequestVolumeThreshold = 3, Injection = @"return false;",FallBackName = "GetDictionaryMethodBreaker", RequestCacheEnabled = false)]
        [InterceptMethod(CachingMethod.Get, Key = "GetDictionary", CacheSectionType = SectionType.ddlCache, Mode = CacheTargetType.Redis, Time = 480)]
        Task<bool> GetDictionary();

       
        Task TryThrowException();

       
        Task PublishThroughEventBusAsync(IntegrationEvent evt1);

        
        [Command(Strategy = StrategyType.Injection,  ShuntStrategy = AddressSelectorMode.HashAlgorithm, ExecutionTimeoutInMilliseconds = 2500, BreakerRequestVolumeThreshold = 3, Injection = @"return null;", RequestCacheEnabled = false)]
        Task<ApiResult<UserModel>> GetApiResult();

        Task<string> GetUser(List<int> idList);
    }
}
