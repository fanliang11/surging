using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Ioc
{
    /// <summary>
    /// Defines the <see cref="ServiceBase" />
    /// </summary>
    public abstract class ServiceBase : IServiceBehavior
    {
        #region 方法

        /// <summary>
        /// The GetService
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The <see cref="T"/></returns>
        public virtual T GetService<T>() where T : class
        {
            return ServiceLocator.GetService<T>();
        }

        /// <summary>
        /// The GetService
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key<see cref="string"/></param>
        /// <returns>The <see cref="T"/></returns>
        public virtual T GetService<T>(string key) where T : class
        {
            return ServiceLocator.GetService<T>(key);
        }

        /// <summary>
        /// The GetService
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="type">The type<see cref="Type"/></param>
        /// <returns>The <see cref="object"/></returns>
        public virtual object GetService(string key, Type type)
        {
            return ServiceLocator.GetService(key, type);
        }

        /// <summary>
        /// The GetService
        /// </summary>
        /// <param name="type">The type<see cref="Type"/></param>
        /// <returns>The <see cref="object"/></returns>
        public virtual object GetService(Type type)
        {
            return ServiceLocator.GetService(type);
        }

        #endregion 方法
    }
}