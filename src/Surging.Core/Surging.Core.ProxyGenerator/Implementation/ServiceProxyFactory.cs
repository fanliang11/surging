﻿using Surging.Core.CPlatform.Convertibles;
using Surging.Core.CPlatform.Runtime.Client;
using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Support;
using System.Collections.Generic;
using Surging.Core.CPlatform.DependencyResolution;
using System.Runtime.CompilerServices;
using Surging.Core.CPlatform.Routing;

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
        private readonly IServiceRouteProvider _serviceRouteProvider;
        private Type[] _serviceTypes=new Type[0];

        #endregion Field

        #region Constructor

        public ServiceProxyFactory(IRemoteInvokeService remoteInvokeService, ITypeConvertibleService typeConvertibleService,
           IServiceProvider serviceProvider, IServiceRouteProvider serviceRouteProvider) :this(remoteInvokeService, typeConvertibleService, serviceProvider, serviceRouteProvider,null, null)
        {

        }

        public ServiceProxyFactory(IRemoteInvokeService remoteInvokeService, ITypeConvertibleService typeConvertibleService,
            IServiceProvider serviceProvider,IServiceRouteProvider serviceRouteProvider, IEnumerable<Type> types, IEnumerable<string> namespaces)
        {
            _serviceRouteProvider = serviceRouteProvider;
            _remoteInvokeService = remoteInvokeService;
            _typeConvertibleService = typeConvertibleService;
            _serviceProvider = serviceProvider;
            if (types != null)
            {
               RegisterProxType(namespaces.ToArray(),types.ToArray());
            }
        }

        #endregion Constructor

        #region Implementation of IServiceProxyFactory

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object CreateProxy(Type type)
        {
            var instance = ServiceResolver.Current.GetService(type);
            if (instance == null)
            {
                var proxyType = _serviceTypes.Single(type.GetTypeInfo().IsAssignableFrom);
                instance = proxyType.GetTypeInfo().GetConstructors().First().Invoke(new object[] { _remoteInvokeService, _typeConvertibleService, null,
             _serviceProvider.GetService<CPlatformContainer>(),_serviceRouteProvider});
                ServiceResolver.Current.Register(null, instance, type);
            }
            return instance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object CreateProxy(string key,Type type)
        {
            var instance = ServiceResolver.Current.GetService(type,key);
            if (instance == null)
            {
                var proxyType = _serviceTypes.Single(type.GetTypeInfo().IsAssignableFrom);
                 instance = proxyType.GetTypeInfo().GetConstructors().First().Invoke(new object[] { _remoteInvokeService, _typeConvertibleService, key,
             _serviceProvider.GetService<CPlatformContainer>(),_serviceRouteProvider});
                ServiceResolver.Current.Register(key, instance, type);
            }
            return instance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T CreateProxy<T>(string key) where T:class
        {
            var instanceType = typeof(T);
            var instance = ServiceResolver.Current.GetService(instanceType, key);
            if (instance == null)
            {
                var proxyType = _serviceTypes.Single(typeof(T).GetTypeInfo().IsAssignableFrom);
                 instance = proxyType.GetTypeInfo().GetConstructors().First().Invoke(new object[] { _remoteInvokeService, _typeConvertibleService,key,
                _serviceProvider.GetService<CPlatformContainer>(),_serviceRouteProvider });
                ServiceResolver.Current.Register(key, instance, instanceType);
            }
            return instance as T;
        }

        public T CreateProxy<T>() where T : class
        {
            return CreateProxy<T>(null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RegisterProxType(string[] namespaces,params Type[] types)
        {
            var proxyGenerater = _serviceProvider.GetService<IServiceProxyGenerater>();
            var serviceTypes = proxyGenerater.GenerateProxys(types, namespaces).ToArray();
            _serviceTypes= _serviceTypes.Except(serviceTypes).Concat(serviceTypes).ToArray();
            proxyGenerater.Dispose();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        #endregion Implementation of IServiceProxyFactory
    }
}