using Surging.Core.Caching.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Surging.Core.Caching.DependencyResolution
{
    /// <summary>
    /// 扩展依赖注入IOC容器
    /// </summary>
    /// <remarks>
    /// 	<para>创建：范亮</para>
    /// 	<para>日期：2016/4/2</para>
    /// </remarks>
    public static class DependencyResolverExtensions
    {
        #region 公共方法

        /// <summary>
        /// 通过KEY获取<see cref="T"/>实例
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="resolver">IOC对象容器</param>
        /// <param name="key">键</param>
        /// <returns>返回<see cref="T"/>实例</returns>
        public static T GetService<T>(this IDependencyResolver resolver, object key)
        {
            Check.NotNull(resolver, "resolver");

            return (T)resolver.GetService(typeof(T), key);
        }

        /// <summary>
        /// 获取<see cref="T"/>实例
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="resolver">IOC对象容器</param>
        /// <returns>返回<see cref="T"/>实例</returns>
        public static T GetService<T>(this IDependencyResolver resolver)
        {
            Check.NotNull(resolver, "resolver");
            return (T)resolver.GetService(typeof(T), null);
        }

        /// <summary>
        /// 通过类型获取对象
        /// </summary>
        /// <param name="resolver">IOC对象容器</param>
        /// <param name="type">类型</param>
        /// <returns>返回对象</returns>
        public static object GetService(this IDependencyResolver resolver, Type type)
        {
            Check.NotNull(resolver, "resolver");
            Check.NotNull(type, "type");
            return resolver.GetService(type, null);
        }

        /// <summary>
        /// 通过KEY获取<see cref="T"/>集合
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="resolver">IOC对象容器</param>
        /// <param name="key">键</param>
        /// <returns>返回<see cref="T"/>实例</returns>
        public static IEnumerable<T> GetServices<T>(this IDependencyResolver resolver, object key)
        {
            Check.NotNull(resolver, "resolver");
            return resolver.GetServices(typeof(T), key).OfType<T>();
        }

        /// <summary>
        /// 获取<see cref="T"/>集合
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="resolver">IOC对象容器</param>
        /// <returns>返回<see cref="T"/>集合</returns>
        public static IEnumerable<T> GetServices<T>(this IDependencyResolver resolver)
        {
            Check.NotNull(resolver, "resolver");
            return resolver.GetServices(typeof(T), null).OfType<T>();
        }

        /// <summary>
        /// 通过类型获取对象集合
        /// </summary>
        /// <param name="resolver">IOC对象容器</param>
        /// <param name="type">类型</param>
        /// <returns>返回集合</returns>
        public static IEnumerable<object> GetServices(this IDependencyResolver resolver, Type type)
        {
            Check.NotNull(resolver, "resolver");
            Check.NotNull(type, "type");
            return resolver.GetServices(type, null);
        }

        #endregion

        /// <summary>
        /// 通过KEY和TYPE获取实例对象集合
        /// </summary>
        /// <param name="resolver">IOC对象容器</param>
        /// <param name="type">类型</param>
        /// <param name="key">键</param>
        /// <returns>返回实例对象集合</returns>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        internal static IEnumerable<object> GetServiceAsServices(this IDependencyResolver resolver, Type type,
            object key)
        {
            DebugCheck.NotNull(resolver);

            var service = resolver.GetService(type, key);
            return service == null ? Enumerable.Empty<object>() : new[] { service };
        }
    }
}
