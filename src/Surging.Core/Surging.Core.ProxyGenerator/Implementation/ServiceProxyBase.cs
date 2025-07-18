﻿using Newtonsoft.Json.Linq;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Convertibles;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Runtime.Client;
using Surging.Core.CPlatform.Support;
using Surging.Core.ProxyGenerator.Interceptors;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Surging.Core.CPlatform.Utilities;
using System.Linq;
using Surging.Core.CPlatform.Routing;
using Surging.Core.ProxyGenerator.Interceptors.Implementation;

namespace Surging.Core.ProxyGenerator.Implementation
{
    /// <summary>
    /// 一个抽象的服务代理基类。
    /// </summary>
    public abstract class ServiceProxyBase
    {
        #region Field
        private readonly IRemoteInvokeService _remoteInvokeService;
        private readonly ITypeConvertibleService _typeConvertibleService;
        private readonly string _serviceKey;
        private readonly CPlatformContainer _serviceProvider;
        private readonly IServiceRouteProvider _serviceRouteProvider;
        private readonly IServiceCommandProvider _commandProvider;
        private readonly IBreakeRemoteInvokeService _breakeRemoteInvokeService;
        private readonly IEnumerable<IInterceptor> _interceptors; 
        #endregion Field

        #region Constructor

        protected ServiceProxyBase(IRemoteInvokeService remoteInvokeService,
            ITypeConvertibleService typeConvertibleService, String serviceKey, CPlatformContainer serviceProvider, IServiceRouteProvider serviceRouteProvider)
        {
            _remoteInvokeService = remoteInvokeService;
            _typeConvertibleService = typeConvertibleService;
            _serviceKey = serviceKey;
            _serviceProvider = serviceProvider;
            _commandProvider = serviceProvider.GetInstances<IServiceCommandProvider>();
            _breakeRemoteInvokeService = serviceProvider.GetInstances<IBreakeRemoteInvokeService>();
            _serviceRouteProvider = serviceRouteProvider;
            _interceptors = new List<IInterceptor>();
            if (serviceProvider.Current.IsRegistered<IInterceptor>())
            { 
                _interceptors = serviceProvider.GetInstances<IEnumerable<IInterceptor>>(); 
            }
        }
        #endregion Constructor

        #region Protected Method
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
            var decodeJOject = typeof(T) == UtilityType.ObjectType|| typeof(T) == UtilityType.JObjectType;
            IInvocation invocation = null;
            var serviceRoute =await _serviceRouteProvider.Locate(serviceId);
            var enableStageCache = serviceRoute.ServiceDescriptor.GetCacheIntercept("Cache")?.EnableStageCache;
            if ((serviceRoute ==null || !serviceRoute.ServiceDescriptor.ExistIntercept()) ||
              (enableStageCache == false && decodeJOject))
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
            if (serviceRoute != null && serviceRoute.ServiceDescriptor.ExistIntercept())
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
                else  result = _typeConvertibleService.Convert(message.Result, typeof(T));
            }
            return (T)result;
        }

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
                var vt =  _commandProvider.GetCommand(serviceId); 
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
        /// <param name="parameters">参数字典。</param>
        /// <param name="serviceId">服务Id。</param>
        /// <returns>调用任务。</returns>
        protected async Task Invoke(IDictionary<string, object> parameters, string serviceId)
        {
            var existsInterceptor = _interceptors.Any();
            RemoteInvokeResultMessage message = null;
            var serviceRoute = await _serviceRouteProvider.Locate(serviceId);
            if (serviceRoute == null || !serviceRoute.ServiceDescriptor.ExistIntercept())
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
                var vt =   _commandProvider.GetCommand(serviceId);
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

        private async Task<Tuple<RemoteInvokeResultMessage, object>> Intercept(IInterceptor interceptor, IInvocation invocation)
        {
            await interceptor.Intercept(invocation);
            var message = invocation.ReturnValue is RemoteInvokeResultMessage
             ? invocation.ReturnValue as RemoteInvokeResultMessage : null;
            return new Tuple<RemoteInvokeResultMessage, object>(message, invocation.ReturnValue);
        }
        
        private IInvocation GetInvocation(IDictionary<string, object> parameters, string serviceId, Type returnType)
        {
            var invocation = _serviceProvider.GetInstances<IInterceptorProvider>();
            return invocation.GetInvocation(this, parameters, serviceId, returnType);
        }

        private IInvocation GetCacheInvocation(IDictionary<string, object> parameters, string serviceId, Type returnType)
        {
            var invocation = _serviceProvider.GetInstances<IInterceptorProvider>();
            return invocation.GetCacheInvocation(this, parameters, serviceId, returnType);
        }

        #endregion Protected Method
    }
}