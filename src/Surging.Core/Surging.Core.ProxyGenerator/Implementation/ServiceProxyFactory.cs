using Microsoft.Extensions.DependencyInjection;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Convertibles;
using Surging.Core.CPlatform.DependencyResolution;
using Surging.Core.CPlatform.Runtime.Client;
using Surging.Core.CPlatform.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Surging.Core.ProxyGenerator.Implementation
{
    /// <summary>
    /// 默认的服务代理工厂实现。
    /// </summary>
    public class ServiceProxyFactory : IServiceProxyFactory
    {
        #region 字段

        /// <summary>
        /// Defines the _remoteInvokeService
        /// </summary>
        private readonly IRemoteInvokeService _remoteInvokeService;

        /// <summary>
        /// Defines the _serviceProvider
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Defines the _typeConvertibleService
        /// </summary>
        private readonly ITypeConvertibleService _typeConvertibleService;

        /// <summary>
        /// Defines the _serviceTypes
        /// </summary>
        private Type[] _serviceTypes = new Type[0];

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProxyFactory"/> class.
        /// </summary>
        /// <param name="remoteInvokeService">The remoteInvokeService<see cref="IRemoteInvokeService"/></param>
        /// <param name="typeConvertibleService">The typeConvertibleService<see cref="ITypeConvertibleService"/></param>
        /// <param name="serviceProvider">The serviceProvider<see cref="IServiceProvider"/></param>
        /// <param name="types">The types<see cref="IEnumerable{Type}"/></param>
        /// <param name="namespaces">The namespaces<see cref="IEnumerable{string}"/></param>
        public ServiceProxyFactory(IRemoteInvokeService remoteInvokeService, ITypeConvertibleService typeConvertibleService,
            IServiceProvider serviceProvider, IEnumerable<Type> types, IEnumerable<string> namespaces)
        {
            _remoteInvokeService = remoteInvokeService;
            _typeConvertibleService = typeConvertibleService;
            _serviceProvider = serviceProvider;
            if (types != null)
            {
                RegisterProxType(namespaces.ToArray(), types.ToArray());
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProxyFactory"/> class.
        /// </summary>
        /// <param name="remoteInvokeService">The remoteInvokeService<see cref="IRemoteInvokeService"/></param>
        /// <param name="typeConvertibleService">The typeConvertibleService<see cref="ITypeConvertibleService"/></param>
        /// <param name="serviceProvider">The serviceProvider<see cref="IServiceProvider"/></param>
        public ServiceProxyFactory(IRemoteInvokeService remoteInvokeService, ITypeConvertibleService typeConvertibleService,
           IServiceProvider serviceProvider) : this(remoteInvokeService, typeConvertibleService, serviceProvider, null, null)
        {
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The CreateProxy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The <see cref="T"/></returns>
        public T CreateProxy<T>() where T : class
        {
            return CreateProxy<T>(null);
        }

        /// <summary>
        /// The CreateProxy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key<see cref="string"/></param>
        /// <returns>The <see cref="T"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T CreateProxy<T>(string key) where T : class
        {
            var instanceType = typeof(T);
            var instance = ServiceResolver.Current.GetService(instanceType, key);
            if (instance == null)
            {
                var proxyType = _serviceTypes.Single(typeof(T).GetTypeInfo().IsAssignableFrom);
                instance = proxyType.GetTypeInfo().GetConstructors().First().Invoke(new object[] { _remoteInvokeService, _typeConvertibleService,key,
                _serviceProvider.GetService<CPlatformContainer>() });
                ServiceResolver.Current.Register(key, instance, instanceType);
            }
            return instance as T;
        }

        /// <summary>
        /// The CreateProxy
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="type">The type<see cref="Type"/></param>
        /// <returns>The <see cref="object"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object CreateProxy(string key, Type type)
        {
            var instance = ServiceResolver.Current.GetService(type, key);
            if (instance == null)
            {
                var proxyType = _serviceTypes.Single(type.GetTypeInfo().IsAssignableFrom);
                instance = proxyType.GetTypeInfo().GetConstructors().First().Invoke(new object[] { _remoteInvokeService, _typeConvertibleService, key,
             _serviceProvider.GetService<CPlatformContainer>()});
                ServiceResolver.Current.Register(key, instance, type);
            }
            return instance;
        }

        /// <summary>
        /// The CreateProxy
        /// </summary>
        /// <param name="type">The type<see cref="Type"/></param>
        /// <returns>The <see cref="object"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object CreateProxy(Type type)
        {
            var instance = ServiceResolver.Current.GetService(type);
            if (instance == null)
            {
                var proxyType = _serviceTypes.Single(type.GetTypeInfo().IsAssignableFrom);
                instance = proxyType.GetTypeInfo().GetConstructors().First().Invoke(new object[] { _remoteInvokeService, _typeConvertibleService, null,
             _serviceProvider.GetService<CPlatformContainer>()});
                ServiceResolver.Current.Register(null, instance, type);
            }
            return instance;
        }

        /// <summary>
        /// The RegisterProxType
        /// </summary>
        /// <param name="namespaces">The namespaces<see cref="string[]"/></param>
        /// <param name="types">The types<see cref="Type[]"/></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RegisterProxType(string[] namespaces, params Type[] types)
        {
            var proxyGenerater = _serviceProvider.GetService<IServiceProxyGenerater>();
            var serviceTypes = proxyGenerater.GenerateProxys(types, namespaces).ToArray();
            _serviceTypes = _serviceTypes.Except(serviceTypes).Concat(serviceTypes).ToArray();
            proxyGenerater.Dispose();
        }

        #endregion 方法
    }
}