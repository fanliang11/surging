using System;

namespace Surging.Core.ProxyGenerator
{
    #region 接口

    /// <summary>
    /// 一个抽象的服务代理工厂。
    /// </summary>
    public interface IServiceProxyFactory
    {
        #region 方法

        /// <summary>
        /// The CreateProxy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The <see cref="T"/></returns>
        T CreateProxy<T>() where T : class;

        /// <summary>
        /// The CreateProxy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key<see cref="string"/></param>
        /// <returns>The <see cref="T"/></returns>
        T CreateProxy<T>(string key) where T : class;

        /// <summary>
        /// The CreateProxy
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="type">The type<see cref="Type"/></param>
        /// <returns>The <see cref="object"/></returns>
        object CreateProxy(string key, Type type);

        /// <summary>
        /// 创建服务代理。
        /// </summary>
        /// <param name="proxyType">代理类型。</param>
        /// <returns>服务代理实例。</returns>
        object CreateProxy(Type proxyType);

        /// <summary>
        /// The RegisterProxType
        /// </summary>
        /// <param name="namespaces">The namespaces<see cref="string[]"/></param>
        /// <param name="types">The types<see cref="Type[]"/></param>
        void RegisterProxType(string[] namespaces, params Type[] types);

        #endregion 方法
    }

    #endregion 接口

    /// <summary>
    /// 服务代理工厂扩展。
    /// </summary>
    public static class ServiceProxyFactoryExtensions
    {
        #region 方法

        /// <summary>
        /// 创建服务代理。
        /// </summary>
        /// <typeparam name="T">服务接口类型。</typeparam>
        /// <param name="serviceProxyFactory">服务代理工厂。</param>
        /// <param name="proxyType">代理类型。</param>
        /// <returns>服务代理实例。</returns>
        public static T CreateProxy<T>(this IServiceProxyFactory serviceProxyFactory, Type proxyType)
        {
            return (T)serviceProxyFactory.CreateProxy(proxyType);
        }

        #endregion 方法
    }
}