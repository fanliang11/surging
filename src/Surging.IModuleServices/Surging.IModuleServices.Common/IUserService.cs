using Surging.Core.Caching;
using Surging.Core.Common;
using Surging.Core.CPlatform.EventBus.Events;
using Surging.Core.CPlatform.Filters.Implementation;
using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors.Implementation;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Surging.Core.CPlatform.Support;
using Surging.Core.CPlatform.Support.Attributes;
using Surging.Core.KestrelHttpServer;
using Surging.Core.KestrelHttpServer.Internal;
using Surging.Core.System.Intercept;
using Surging.IModuleServices.Common.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Surging.IModuleServices.Common
{
    [ServiceBundle("api/{Service}/{Method}")]
    //[ServiceBundle("api/{Service}")]
    //[ServiceBundle("api/{Service}/{Method}/test")]
    //[ServiceBundle("api/{Service}/{Method}/test",false)]
    public interface IUserService: IServiceKey
    {
        /// <summary>
        /// 用戶授权
        /// </summary>
        /// <param name="requestData">请求参数</param>
        /// <returns>用户模型</returns>
        Task<UserModel> Authentication(AuthenticationRequestData requestData);

        /// <summary>
        /// 获取用户姓名
        /// </summary>
        /// <param name="id">用户编号</param>
        /// <returns></returns>
        [ServiceRoute("{id}")]
        Task<string> GetUserName(int id);

        /// <summary>
        /// 判断是否存在
        /// </summary>
        /// <param name="id">用户编号</param>
        /// <returns></returns>
        [ServiceRoute("{id}")]
       // [ServiceBundle("api/{Service}/{id}", false)]
        Task<bool> Exists(int id);

        /// <summary>
        /// 报错用户
        /// </summary>
        /// <param name="requestData">请求参数</param>
        /// <returns></returns>
        [Authorization(AuthType = AuthorizationType.JWT)]
       
        Task<IdentityUser> Save(IdentityUser requestData);

        /// <summary>
        /// 根据用户名获取用户ID
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <returns></returns>
        [Authorization(AuthType = AuthorizationType.JWT)]
        [Command(Strategy = StrategyType.Injection, ShuntStrategy = AddressSelectorMode.HashAlgorithm, ExecutionTimeoutInMilliseconds = 1500, BreakerRequestVolumeThreshold = 3, Injection = @"return 1;", RequestCacheEnabled = true)]
        [InterceptMethod(CachingMethod.Get, Key = "GetUserId_{0}", CacheSectionType = SectionType.ddlCache, Mode = CacheTargetType.Redis, Time = 480)]
        [ServiceRoute("{userName}")]
        Task<int> GetUserId(string userName);

        Task Try();

        /// <summary>
        /// 获取用户最后次sign时间
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns></returns>
        Task<DateTime> GetUserLastSignInTime(int id);

        /// <summary>
        /// 获取用户
        /// </summary>
        /// <param name="user">用户模型</param>
        /// <returns></returns>
        [Command(Strategy = StrategyType.Injection, Injection = @"return
new Surging.IModuleServices.Common.Models.UserModel
         {
            Name=""fanly"",
            Age=19
         };", RequestCacheEnabled = true, InjectionNamespaces = new string[] { "Surging.IModuleServices.Common" })]
        [InterceptMethod(CachingMethod.Get, Key = "GetUser_id_{0}", CacheSectionType = SectionType.ddlCache, Mode = CacheTargetType.Redis, Time = 480)]
        Task<UserModel> GetUser(UserModel user);

        /// <summary>
        /// 更新用户
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="model">用户模型</param>
        /// <returns></returns>
        [Authorization(AuthType = AuthorizationType.JWT)]
        [Command(Strategy = StrategyType.FallBack,FallBackName = "UpdateFallBackName",  RequestCacheEnabled = true, InjectionNamespaces = new string[] { "Surging.IModuleServices.Common" })]
        [InterceptMethod(CachingMethod.Remove, "GetUser_id_{0}", "GetUserName_name_{0}", CacheSectionType = SectionType.ddlCache, Mode = CacheTargetType.Redis)]
        Task<bool> Update(int id, UserModel model);

        /// <summary>
        /// 测试List参数调用
        /// </summary>
        /// <param name="users">用户列表</param>
        /// <returns>返回是否成功</returns>
        Task<bool> Get(List<UserModel> users);

        /// <summary>
        /// 测试无参数调用
        /// </summary>
        /// <returns>返回是否成功</returns>
        [Command(Strategy = StrategyType.Injection,ShuntStrategy = AddressSelectorMode.Polling, ExecutionTimeoutInMilliseconds = 1500, BreakerRequestVolumeThreshold = 3, Injection = @"return false;",FallBackName = "GetDictionaryMethodBreaker", RequestCacheEnabled = false)]
        [InterceptMethod(CachingMethod.Get, Key = "GetDictionary", CacheSectionType = SectionType.ddlCache, Mode = CacheTargetType.Redis, Time = 480)]
        Task<bool> GetDictionary();

       /// <summary>
       /// 测试异常
       /// </summary>
       /// <returns></returns>
        Task TryThrowException();

        [ServiceRoute("{sex}")]
        Task<Sex> SetSex(Sex sex);

        /// <summary>
        /// 测试基于eventbus 推送消息
        /// </summary>
        /// <param name="evt1">Event 模型</param>
        /// <returns></returns>
        Task PublishThroughEventBusAsync(IntegrationEvent evt1);

        /// <summary>
        /// 测试无参调用，返回泛型结果
        /// </summary>
        /// <returns></returns>
        [Command(Strategy = StrategyType.Injection,  ShuntStrategy = AddressSelectorMode.HashAlgorithm, ExecutionTimeoutInMilliseconds = 2500, BreakerRequestVolumeThreshold = 3, Injection = @"return null;", RequestCacheEnabled = false)]
        Task<ApiResult<UserModel>> GetApiResult();

        /// <summary>
        /// 测试参数list参数
        /// </summary>
        /// <param name="idList">list 类型参数</param>
        /// <returns></returns>
        [ServiceMetadata("IsOverload", true)]
        Task<string> GetUser(List<int> idList);

        /// <summary>
        /// 测序guid
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ServiceRoute("{id}")]
        Task<UserModel> GetUserById(Guid id);

        /// <summary>
        /// 测试上传文件
        /// </summary>
        /// <param name="form">HttpFormCollection 类型参数</param>
        /// <returns></returns>
        Task<bool> UploadFile(HttpFormCollection form);

        /// <summary>
        /// 测试下载文件
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="contentType">Content-Type</param>
        /// <returns></returns>
        [ServiceRoute("{fileName}/{contentType}")]
        Task<IActionResult> DownFile(string fileName, string contentType);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<Dictionary<string, object>> GetAllThings();
    }
}
