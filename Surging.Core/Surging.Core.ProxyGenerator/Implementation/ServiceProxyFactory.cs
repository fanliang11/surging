using Surging.Core.CPlatform.Convertibles;
using Surging.Core.CPlatform.Runtime.Client;
using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Support;

namespace Surging.Core.ProxyGenerator.Implementation
{
    /// <summary>
    /// 默认的服务代理工厂实现。
    /// </summary>
    public class ServiceProxyFactory : IServiceProxyFactory
    {
        #region Field
        private readonly IRemoteInvokeService _remoteInvokeService;
        private readonly ITypeConvertibleService _typeConvertibleService;
        private readonly IServiceProvider _serviceProvider;
        private Type[] _serviceTypes;

        #endregion Field

        #region Constructor

        public ServiceProxyFactory(IRemoteInvokeService remoteInvokeService, ITypeConvertibleService typeConvertibleService,
            IServiceProvider serviceProvider)
        {
            _remoteInvokeService = remoteInvokeService;
            _typeConvertibleService = typeConvertibleService;
            _serviceProvider = serviceProvider;
        }

        #endregion Constructor

        #region Implementation of IServiceProxyFactory

  
        public object CreateProxy(Type type)
        {
            var proxyType = _serviceTypes.Single(type.GetTypeInfo().IsAssignableFrom);
            var instance = proxyType.GetTypeInfo().GetConstructors().First().Invoke(new object[] { _remoteInvokeService, _typeConvertibleService, null,
             _serviceProvider.GetService<CPlatformContainer>()});
            return instance;
        }
        
        public object CreateProxy(string key,Type type)
        {
            var proxyType = _serviceTypes.Single(type.GetTypeInfo().IsAssignableFrom);
            var instance = proxyType.GetTypeInfo().GetConstructors().First().Invoke(new object[] { _remoteInvokeService, _typeConvertibleService, key,
             _serviceProvider.GetService<CPlatformContainer>()});
            return instance;
        }

        public T CreateProxy<T>(string key) where T:class
        {
            var proxyType = _serviceTypes.Single(typeof(T).GetTypeInfo().IsAssignableFrom);
          
            var instance = proxyType.GetTypeInfo().GetConstructors().First().Invoke(new object[] { _remoteInvokeService, _typeConvertibleService,key,
                _serviceProvider.GetService<CPlatformContainer>() });
            return instance as T;
        }

        public T CreateProxy<T>() where T : class
        {
            return CreateProxy<T>(null);
        }

        public void RegisterProxType(params Type[] types)
        {
            _serviceTypes = _serviceProvider.GetService<IServiceProxyGenerater>().GenerateProxys(types).ToArray();
        }

        #endregion Implementation of IServiceProxyFactory
    }
}