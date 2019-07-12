using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Surging.Core.CPlatform.DependencyResolution
{
    /// <summary>
    /// IOC容器对象
    /// </summary>
    public class ServiceResolver : IDependencyResolver
    {
        #region 字段

        /// <summary>
        /// Defines the _defaultInstance
        /// </summary>
        private static readonly ServiceResolver _defaultInstance = new ServiceResolver();

        /// <summary>
        /// Defines the _initializers
        /// </summary>
        private readonly ConcurrentDictionary<ValueTuple<Type, string>, object> _initializers =
            new ConcurrentDictionary<ValueTuple<Type, string>, object>();

        #endregion 字段

        #region 属性

        /// <summary>
        /// Gets the Current
        /// 返回当前IOC容器
        /// </summary>
        public static ServiceResolver Current
        {
            get { return _defaultInstance; }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// 通过KEY和TYPE获取实例对象
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="key">键</param>
        /// <returns>返回实例对象</returns>
        public virtual object GetService(Type type, object key)
        {
            object result;
            _initializers.TryGetValue(ValueTuple.Create(type, key == null ? null : key.ToString()), out result);
            return result;
        }

        /// <summary>
        /// 通过KEY和TYPE获取实例对象集合
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="key">键</param>
        /// <returns>返回实例对象</returns>
        public IEnumerable<object> GetServices(Type type, object key)
        {
            return this.GetServiceAsServices(type, key);
        }

        /// <summary>
        /// 注册对象添加到IOC容器
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public virtual void Register(string key, object value)
        {
            DebugCheck.NotNull(value);
            // DebugCheck.NotNull(key);

            _initializers.GetOrAdd(ValueTuple.Create(value.GetType(), key), value);
            var interFaces = value.GetType().GetTypeInfo().GetInterfaces();
            foreach (var interFace in interFaces)
            {
                _initializers.GetOrAdd(ValueTuple.Create(interFace, key), value);
            }
        }

        /// <summary>
        /// The Register
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="value">The value<see cref="object"/></param>
        /// <param name="type">The type<see cref="Type"/></param>
        public virtual void Register(string key, object value, Type type)
        {
            DebugCheck.NotNull(value);
            _initializers.GetOrAdd(ValueTuple.Create(type, key), value);
        }

        #endregion 方法
    }
}