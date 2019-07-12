using Autofac;
using System;

namespace Surging.Core.CPlatform.Utilities
{
    /// <summary>
    /// Defines the <see cref="ServiceLocator" />
    /// </summary>
    public class ServiceLocator
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Current
        /// </summary>
        public static IContainer Current { get; set; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The GetService
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The <see cref="T"/></returns>
        public static T GetService<T>()
        {
            return Current.Resolve<T>();
        }

        /// <summary>
        /// The GetService
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key<see cref="string"/></param>
        /// <returns>The <see cref="T"/></returns>
        public static T GetService<T>(string key)
        {
            return Current.ResolveKeyed<T>(key);
        }

        /// <summary>
        /// The GetService
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="type">The type<see cref="Type"/></param>
        /// <returns>The <see cref="object"/></returns>
        public static object GetService(string key, Type type)
        {
            return Current.ResolveKeyed(key, type);
        }

        /// <summary>
        /// The GetService
        /// </summary>
        /// <param name="type">The type<see cref="Type"/></param>
        /// <returns>The <see cref="object"/></returns>
        public static object GetService(Type type)
        {
            return Current.Resolve(type);
        }

        /// <summary>
        /// The IsRegistered
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The <see cref="bool"/></returns>
        public static bool IsRegistered<T>()
        {
            return Current.IsRegistered<T>();
        }

        /// <summary>
        /// The IsRegistered
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public static bool IsRegistered<T>(string key)
        {
            return Current.IsRegisteredWithKey<T>(key);
        }

        /// <summary>
        /// The IsRegistered
        /// </summary>
        /// <param name="type">The type<see cref="Type"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public static bool IsRegistered(Type type)
        {
            return Current.IsRegistered(type);
        }

        /// <summary>
        /// The IsRegisteredWithKey
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="type">The type<see cref="Type"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public static bool IsRegisteredWithKey(string key, Type type)
        {
            return Current.IsRegisteredWithKey(key, type);
        }

        #endregion 方法
    }
}