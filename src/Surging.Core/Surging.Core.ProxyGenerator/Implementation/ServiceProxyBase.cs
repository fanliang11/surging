using Autofac;
using Newtonsoft.Json.Linq;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Convertibles;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Runtime.Client;
using Surging.Core.CPlatform.Support;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.ProxyGenerator.Interceptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surging.Core.ProxyGenerator.Implementation
{
    /// <summary>
    /// 一个抽象的服务代理基类。
    /// </summary>
    public abstract class ServiceProxyBase
    {
        #region 字段

        /// <summary>
        /// Defines the _breakeRemoteInvokeService
        /// </summary>
        private readonly IBreakeRemoteInvokeService _breakeRemoteInvokeService;

        /// <summary>
        /// Defines the _cacheInterceptor
        /// </summary>
        private readonly IInterceptor _cacheInterceptor;

        /// <summary>
        /// Defines the _commandProvider
        /// </summary>
        private readonly IServiceCommandProvider _commandProvider;

        /// <summary>
        /// Defines the _interceptors
        /// </summary>
        private readonly IEnumerable<IInterceptor> _interceptors;

        /// <summary>
        /// Defines the _remoteInvokeService
        /// </summary>
        private readonly IRemoteInvokeService _remoteInvokeService;

        /// <summary>
        /// Defines the _serviceKey
        /// </summary>
        private readonly string _serviceKey;

        /// <summary>
        /// Defines the _serviceProvider
        /// </summary>
        private readonly CPlatformContainer _serviceProvider;

        /// <summary>
        /// Defines the _typeConvertibleService
        /// </summary>
        private readonly ITypeConvertibleService _typeConvertibleService;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProxyBase"/> class.
        /// </summary>
        /// <param name="remoteInvokeService">The remoteInvokeService<see cref="IRemoteInvokeService"/></param>
        /// <param name="typeConvertibleService">The typeConvertibleService<see cref="ITypeConvertibleService"/></param>
        /// <param name="serviceKey">The serviceKey<see cref="String"/></param>
        /// <param name="serviceProvider">The serviceProvider<see cref="CPlatformContainer"/></param>
        protected ServiceProxyBase(IRemoteInvokeService remoteInvokeService,
            ITypeConvertibleService typeConvertibleService, String serviceKey, CPlatformContainer serviceProvider)
        {
            _remoteInvokeService = remoteInvokeService;
            _typeConvertibleService = typeConvertibleService;
            _serviceKey = serviceKey;
            _serviceProvider = serviceProvider;
            _commandProvider = serviceProvider.GetInstances<IServiceCommandProvider>();
            _breakeRemoteInvokeService = serviceProvider.GetInstances<IBreakeRemoteInvokeService>();
            _interceptors = new List<IInterceptor>();
            if (serviceProvider.Current.IsRegistered<IInterceptor>())
            {
                var interceptors = serviceProvider.GetInstances<IEnumerable<IInterceptor>>();
                _interceptors = interceptors.Where(p => !typeof(CacheInterceptor).IsAssignableFrom(p.GetType()));
                _cacheInterceptor = interceptors.Where(p => typeof(CacheInterceptor).IsAssignableFrom(p.GetType())).FirstOrDefault();
            }
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The CallInvoke
        /// </summary>
        /// <param name="invocation">The invocation<see cref="IInvocation"/></param>
        /// <returns>The <see cref="Task{object}"/></returns>
        public async Task<object> CallInvoke(IInvocation invocation)
        {
            var cacheInvocation = invocation as ICacheInvocation;
            var parameters = invocation.Arguments;
            var serviceId = invocation.ServiceId;
            var type = invocation.ReturnType;
            var message = await _breakeRemoteInvokeService.InvokeAsync(parameters, serviceId, _serviceKey,
                   type == typeof(Task) ? false : true);
            if (message == null)
            {
                var vt = _commandProvider.GetCommand(serviceId);
                var command = vt.IsCompletedSuccessfully ? vt.Result : await vt;
                if (command.FallBackName != null && _serviceProvider.IsRegistered<IFallbackInvoker>(command.FallBackName) && command.Strategy == StrategyType.FallBack)
                {
                    var invoker = _serviceProvider.GetInstances<IFallbackInvoker>(command.FallBackName);
                    return await invoker.Invoke<object>(parameters, serviceId, _serviceKey);
                }
                else
                {
                    var invoker = _serviceProvider.GetInstances<IClusterInvoker>(command.Strategy.ToString());
                    return await invoker.Invoke<object>(parameters, serviceId, _serviceKey, true);
                }
            }
            if (type == typeof(Task)) return message;
            return _typeConvertibleService.Convert(message.Result, type);
        }

        /// <summary>
        /// 远程调用。
        /// </summary>
        /// <typeparam name="T">返回类型。</typeparam>
        /// <param name="parameters">参数字典。</param>
        /// <param name="serviceId">服务Id。</param>
        /// <returns>调用结果。</returns>
        protected async Task<T> Invoke<T>(IDictionary<string, object> parameters, string serviceId)
        {
            object result = default(T);
            var vt = _commandProvider.GetCommand(serviceId);
            var command = vt.IsCompletedSuccessfully ? vt.Result : await vt;
            RemoteInvokeResultMessage message = null;
            var decodeJOject = typeof(T) == UtilityType.ObjectType;
            IInvocation invocation = null;
            var existsInterceptor = _interceptors.Any();
            if ((_cacheInterceptor == null || !command.RequestCacheEnabled) && !existsInterceptor)
            {
                message = await _breakeRemoteInvokeService.InvokeAsync(parameters, serviceId, _serviceKey, decodeJOject);
                if (message == null)
                {
                    if (command.FallBackName != null && _serviceProvider.IsRegistered<IFallbackInvoker>(command.FallBackName) && command.Strategy == StrategyType.FallBack)
                    {
                        var invoker = _serviceProvider.GetInstances<IFallbackInvoker>(command.FallBackName);
                        return await invoker.Invoke<T>(parameters, serviceId, _serviceKey);
                    }
                    else
                    {
                        var invoker = _serviceProvider.GetInstances<IClusterInvoker>(command.Strategy.ToString());
                        return await invoker.Invoke<T>(parameters, serviceId, _serviceKey, typeof(T) == UtilityType.ObjectType);
                    }
                }
            }
            if (_cacheInterceptor != null && command.RequestCacheEnabled)
            {
                invocation = GetCacheInvocation(parameters, serviceId, typeof(T));
                var interceptReuslt = await Intercept(_cacheInterceptor, invocation);
                message = interceptReuslt.Item1;
                result = interceptReuslt.Item2 == null ? default(T) : interceptReuslt.Item2;
            }
            if (existsInterceptor)
            {
                invocation = invocation == null ? GetInvocation(parameters, serviceId, typeof(T)) : invocation;
                foreach (var interceptor in _interceptors)
                {
                    var interceptReuslt = await Intercept(interceptor, invocation);
                    message = interceptReuslt.Item1;
                    result = interceptReuslt.Item2 == null ? default(T) : interceptReuslt.Item2;
                }
            }
            if (message != null)
            {
                if (message.Result == null) result = message.Result;
                else result = _typeConvertibleService.Convert(message.Result, typeof(T));
            }
            return (T)result;
        }

        /// <summary>
        /// 远程调用。
        /// </summary>
        /// <param name="parameters">参数字典。</param>
        /// <param name="serviceId">服务Id。</param>
        /// <returns>调用任务。</returns>
        protected async Task Invoke(IDictionary<string, object> parameters, string serviceId)
        {
            var existsInterceptor = _interceptors.Any();
            RemoteInvokeResultMessage message = null;
            if (!existsInterceptor)
                message = await _breakeRemoteInvokeService.InvokeAsync(parameters, serviceId, _serviceKey, false);
            else
            {
                var invocation = GetInvocation(parameters, serviceId, typeof(Task));
                foreach (var interceptor in _interceptors)
                {
                    var interceptReuslt = await Intercept(interceptor, invocation);
                    message = interceptReuslt.Item1;
                }
            }
            if (message == null)
            {
                var vt = _commandProvider.GetCommand(serviceId);
                var command = vt.IsCompletedSuccessfully ? vt.Result : await vt;
                if (command.FallBackName != null && _serviceProvider.IsRegistered<IFallbackInvoker>(command.FallBackName) && command.Strategy == StrategyType.FallBack)
                {
                    var invoker = _serviceProvider.GetInstances<IFallbackInvoker>(command.FallBackName);
                    await invoker.Invoke<object>(parameters, serviceId, _serviceKey);
                }
                else
                {
                    var invoker = _serviceProvider.GetInstances<IClusterInvoker>(command.Strategy.ToString());
                    await invoker.Invoke(parameters, serviceId, _serviceKey, true);
                }
            }
        }

        /// <summary>
        /// The GetCacheInvocation
        /// </summary>
        /// <param name="parameters">The parameters<see cref="IDictionary{string, object}"/></param>
        /// <param name="serviceId">The serviceId<see cref="string"/></param>
        /// <param name="returnType">The returnType<see cref="Type"/></param>
        /// <returns>The <see cref="IInvocation"/></returns>
        private IInvocation GetCacheInvocation(IDictionary<string, object> parameters, string serviceId, Type returnType)
        {
            var invocation = _serviceProvider.GetInstances<IInterceptorProvider>();
            return invocation.GetCacheInvocation(this, parameters, serviceId, returnType);
        }

        /// <summary>
        /// The GetInvocation
        /// </summary>
        /// <param name="parameters">The parameters<see cref="IDictionary{string, object}"/></param>
        /// <param name="serviceId">The serviceId<see cref="string"/></param>
        /// <param name="returnType">The returnType<see cref="Type"/></param>
        /// <returns>The <see cref="IInvocation"/></returns>
        private IInvocation GetInvocation(IDictionary<string, object> parameters, string serviceId, Type returnType)
        {
            var invocation = _serviceProvider.GetInstances<IInterceptorProvider>();
            return invocation.GetInvocation(this, parameters, serviceId, returnType);
        }

        /// <summary>
        /// The Intercept
        /// </summary>
        /// <param name="interceptor">The interceptor<see cref="IInterceptor"/></param>
        /// <param name="invocation">The invocation<see cref="IInvocation"/></param>
        /// <returns>The <see cref="Task{Tuple{RemoteInvokeResultMessage, object}}"/></returns>
        private async Task<Tuple<RemoteInvokeResultMessage, object>> Intercept(IInterceptor interceptor, IInvocation invocation)
        {
            await interceptor.Intercept(invocation);
            var message = invocation.ReturnValue is RemoteInvokeResultMessage
             ? invocation.ReturnValue as RemoteInvokeResultMessage : null;
            return new Tuple<RemoteInvokeResultMessage, object>(message, invocation.ReturnValue);
        }

        #endregion 方法
    }
}