using Autofac;
using Surging.Core.CPlatform.DependencyResolution;
using System;

namespace Surging.Core.CPlatform
{
    /// <summary>
    /// 平台容器
    /// </summary>
    public class CPlatformContainer
    {
        #region 字段

        /// <summary>
        /// Defines the _container
        /// </summary>
        private IComponentContext _container;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="CPlatformContainer"/> class.
        /// </summary>
        /// <param name="container">The container<see cref="IComponentContext"/></param>
        public CPlatformContainer(IComponentContext container)
        {
            this._container = container;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the Current
        /// </summary>
        public IComponentContext Current
        {
            get
            {
                return _container;
            }
            internal set
            {
                _container = value;
            }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The GetInstancePerLifetimeScope
        /// </summary>
        /// <param name="name">The name<see cref="string"/></param>
        /// <param name="type">The type<see cref="Type"/></param>
        /// <returns>The <see cref="object"/></returns>
        public object GetInstancePerLifetimeScope(string name, Type type)
        {
            return string.IsNullOrEmpty(name) ? GetInstances(type) : _container.ResolveKeyed(name, type);
        }

        /// <summary>
        /// The GetInstances
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The <see cref="T"/></returns>
        public T GetInstances<T>() where T : class
        {
            return _container.Resolve<T>();
        }

        /// <summary>
        /// The GetInstances
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name<see cref="string"/></param>
        /// <returns>The <see cref="T"/></returns>
        public T GetInstances<T>(string name) where T : class
        {
            return _container.ResolveKeyed<T>(name);
        }

        /// <summary>
        /// The GetInstances
        /// </summary>
        /// <param name="name">The name<see cref="string"/></param>
        /// <param name="type">The type<see cref="Type"/></param>
        /// <returns>The <see cref="object"/></returns>
        public object GetInstances(string name, Type type)
        {
            // var appConfig = AppConfig.DefaultInstance;
            var objInstance = ServiceResolver.Current.GetService(type, name);
            if (objInstance == null)
            {
                objInstance = string.IsNullOrEmpty(name) ? GetInstances(type) : _container.ResolveKeyed(name, type);
                ServiceResolver.Current.Register(name, objInstance, type);
            }
            return objInstance;
        }

        /// <summary>
        /// The GetInstances
        /// </summary>
        /// <param name="type">The type<see cref="Type"/></param>
        /// <returns>The <see cref="object"/></returns>
        public object GetInstances(Type type)
        {
            return _container.Resolve(type);
        }

        /// <summary>
        /// The GetInstances
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">The type<see cref="Type"/></param>
        /// <returns>The <see cref="T"/></returns>
        public T GetInstances<T>(Type type) where T : class
        {
            return _container.Resolve(type) as T;
        }

        /// <summary>
        /// The IsRegistered
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The <see cref="bool"/></returns>
        public bool IsRegistered<T>()
        {
            return _container.IsRegistered<T>();
        }

        /// <summary>
        /// The IsRegistered
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceKey">The serviceKey<see cref="object"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public bool IsRegistered<T>(object serviceKey)
        {
            return _container.IsRegisteredWithKey<T>(serviceKey);
        }

        /// <summary>
        /// The IsRegisteredWithKey
        /// </summary>
        /// <param name="serviceKey">The serviceKey<see cref="string"/></param>
        /// <param name="serviceType">The serviceType<see cref="Type"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public bool IsRegisteredWithKey(string serviceKey, Type serviceType)
        {
            if (!string.IsNullOrEmpty(serviceKey))
                return _container.IsRegisteredWithKey(serviceKey, serviceType);
            else
                return _container.IsRegistered(serviceType);
        }

        #endregion 方法
    }
}