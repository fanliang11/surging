using Newtonsoft.Json.Linq;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Convertibles;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Runtime.Client;
using Surging.Core.CPlatform.Support;
using Surging.Core.ProxyGenerator.Interceptors;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        private readonly IServiceCommandProvider _commandProvider;
        private readonly IBreakeRemoteInvokeService _breakeRemoteInvokeService;
        private readonly IInterceptor _interceptor;
        #endregion Field

        #region Constructor

        protected ServiceProxyBase(IRemoteInvokeService remoteInvokeService, 
            ITypeConvertibleService typeConvertibleService,String serviceKey, CPlatformContainer serviceProvider)
        {
            _remoteInvokeService = remoteInvokeService;
            _typeConvertibleService = typeConvertibleService;
            _serviceKey = serviceKey;
            _serviceProvider = serviceProvider;
            _commandProvider = serviceProvider.GetInstances<IServiceCommandProvider>();
            _breakeRemoteInvokeService = serviceProvider.GetInstances<IBreakeRemoteInvokeService>();
            _interceptor= serviceProvider.GetInstances<IInterceptor>();
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
            var command = await _commandProvider.GetCommand(serviceId);
            RemoteInvokeResultMessage message;
            var decodeJOject = typeof(T) == typeof(Object);
            if (!command.RequestCacheEnabled || decodeJOject)
            {
                var v = typeof(T).FullName;
                message = await _breakeRemoteInvokeService.InvokeAsync(parameters, serviceId, _serviceKey, decodeJOject);
                if (message == null)
                {
                    var invoker = _serviceProvider.GetInstances<IClusterInvoker>(command.Strategy.ToString());
                    return await invoker.Invoke<T>(parameters, serviceId, _serviceKey, typeof(T) == typeof(Object));
                }
            }
            else
            {
                var invocation = GetInvocation(parameters, serviceId, typeof(T));
                await _interceptor.Intercept(invocation);
                message = invocation.ReturnValue is RemoteInvokeResultMessage
                    ? invocation.ReturnValue as RemoteInvokeResultMessage : null;
                result = invocation.ReturnValue;
            }

            if (message != null)
                result = _typeConvertibleService.Convert(message.Result, typeof(T));
            return (T)result;
        }

        public async Task<object> CallInvoke(IDictionary<string, object> parameters, string serviceId)
        {
            var task = _breakeRemoteInvokeService.InvokeAsync(parameters, serviceId, _serviceKey, true);
            task.Wait();
            if (task.Result == null)
            {
                var command = await _commandProvider.GetCommand(serviceId);
                var invoker = _serviceProvider.GetInstances<IClusterInvoker>(command.Strategy.ToString());
                return await invoker.Invoke<object>(parameters, serviceId, _serviceKey, true);
            }
            return task.Result;
        }

        /// <summary>
        /// 远程调用。
        /// </summary>
        /// <param name="parameters">参数字典。</param>
        /// <param name="serviceId">服务Id。</param>
        /// <returns>调用任务。</returns>
        protected async Task Invoke(IDictionary<string, object> parameters, string serviceId)
        {
            var message = _breakeRemoteInvokeService.InvokeAsync(parameters, serviceId, _serviceKey,false);
            if (message == null)
            {
                var command =await _commandProvider.GetCommand(serviceId);
                var invoker = _serviceProvider.GetInstances<IClusterInvoker>(command.Strategy.ToString());
                await invoker.Invoke(parameters, serviceId, _serviceKey,true);
            }
        }
       
        private IInvocation GetInvocation(IDictionary<string, object> parameters, string serviceId, Type returnType)
        {
            var invocation = _serviceProvider.GetInstances<IInterceptorProvider>();
            return invocation.GetInvocation(this, parameters, serviceId, returnType);
        }
        
        #endregion Protected Method
    }
}