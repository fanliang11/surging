using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Caching
{
    /// <summary>
    /// Defines the <see cref="CacheContainer" />
    /// </summary>
    public class CacheContainer
    {
        #region 方法

        /// <summary>
        /// The GetInstances
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The <see cref="T"/></returns>
        public static T GetInstances<T>() where T : class
        {
            var appConfig = AppConfig.DefaultInstance;
            return appConfig.GetContextInstance<T>();
        }

        /// <summary>
        /// The GetInstances
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name<see cref="string"/></param>
        /// <returns>The <see cref="T"/></returns>
        public static T GetInstances<T>(string name) where T : class
        {
            var appConfig = AppConfig.DefaultInstance;
            return appConfig.GetContextInstance<T>(name);
        }

        /// <summary>
        /// The GetService
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The <see cref="T"/></returns>
        public static T GetService<T>()
        {
            if (ServiceLocator.Current == null) return default(T);
            return ServiceLocator.GetService<T>();
        }

        /// <summary>
        /// The GetService
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name<see cref="string"/></param>
        /// <returns>The <see cref="T"/></returns>
        public static T GetService<T>(string name)
        {
            if (ServiceLocator.Current == null) return default(T);
            return ServiceLocator.GetService<T>(name);
        }

        /// <summary>
        /// The IsRegistered
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The <see cref="bool"/></returns>
        public static bool IsRegistered<T>()
        {
            return ServiceLocator.IsRegistered<T>();
        }

        /// <summary>
        /// The IsRegistered
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public static bool IsRegistered<T>(string key)
        {
            return ServiceLocator.IsRegistered<T>(key);
        }

        /// <summary>
        /// The IsRegistered
        /// </summary>
        /// <param name="type">The type<see cref="Type"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public static bool IsRegistered(Type type)
        {
            return ServiceLocator.IsRegistered(type);
        }

        /// <summary>
        /// The IsRegisteredWithKey
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="type">The type<see cref="Type"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public static bool IsRegisteredWithKey(string key, Type type)
        {
            return ServiceLocator.IsRegisteredWithKey(key, type);
        }

        #endregion 方法
    }
}