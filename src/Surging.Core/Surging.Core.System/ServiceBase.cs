using Autofac;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.ProxyGenerator;
using Surging.Core.ProxyGenerator.Utilitys;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.System
{
    /// <summary>
    /// Defines the <see cref="ServiceBase" />
    /// </summary>
    public abstract class ServiceBase
    {
        #region 方法

        /// <summary>
        /// The CreateProxy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The <see cref="T"/></returns>
        public T CreateProxy<T>() where T : class
        {
            return ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy<T>();
        }

        /// <summary>
        /// The CreateProxy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key<see cref="string"/></param>
        /// <returns>The <see cref="T"/></returns>
        public T CreateProxy<T>(string key) where T : class
        {
            return ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy<T>(key);
        }

        /// <summary>
        /// The CreateProxy
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="type">The type<see cref="Type"/></param>
        /// <returns>The <see cref="object"/></returns>
        public object CreateProxy(string key, Type type)
        {
            return ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy(key, type);
        }

        /// <summary>
        /// The CreateProxy
        /// </summary>
        /// <param name="type">The type<see cref="Type"/></param>
        /// <returns>The <see cref="object"/></returns>
        public object CreateProxy(Type type)
        {
            return ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy(type);
        }

        /// <summary>
        /// The GetService
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The <see cref="T"/></returns>
        public T GetService<T>()
        {
            return ServiceLocator.GetService<T>();
        }

        /// <summary>
        /// The GetService
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key<see cref="string"/></param>
        /// <returns>The <see cref="T"/></returns>
        public T GetService<T>(string key)
        {
            return ServiceLocator.GetService<T>(key);
        }

        /// <summary>
        /// The GetService
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="type">The type<see cref="Type"/></param>
        /// <returns>The <see cref="object"/></returns>
        public object GetService(string key, Type type)
        {
            return ServiceLocator.GetService(key, type);
        }

        /// <summary>
        /// The GetService
        /// </summary>
        /// <param name="type">The type<see cref="Type"/></param>
        /// <returns>The <see cref="object"/></returns>
        public object GetService(Type type)
        {
            return ServiceLocator.GetService(type);
        }

        #endregion 方法
    }
}