using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.ProxyGenerator.Utilitys;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Surging.Core.ProxyGenerator.Interceptors.Implementation
{
    /// <summary>
    /// Defines the <see cref="InterceptorProvider" />
    /// </summary>
    public class InterceptorProvider : IInterceptorProvider
    {
        #region 字段

        /// <summary>
        /// Defines the _serviceEntryManager
        /// </summary>
        private readonly IServiceEntryManager _serviceEntryManager;

        /// <summary>
        /// Defines the _derivedTypes
        /// </summary>
        internal ConcurrentDictionary<Tuple<Type, Type>, bool> _derivedTypes = new ConcurrentDictionary<Tuple<Type, Type>, bool>();

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptorProvider"/> class.
        /// </summary>
        /// <param name="serviceEntryManager">The serviceEntryManager<see cref="IServiceEntryManager"/></param>
        public InterceptorProvider(IServiceEntryManager serviceEntryManager)
        {
            _serviceEntryManager = serviceEntryManager;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The GetCacheInvocation
        /// </summary>
        /// <param name="proxy">The proxy<see cref="object"/></param>
        /// <param name="parameters">The parameters<see cref="IDictionary{string, object}"/></param>
        /// <param name="serviceId">The serviceId<see cref="string"/></param>
        /// <param name="returnType">The returnType<see cref="Type"/></param>
        /// <returns>The <see cref="IInvocation"/></returns>
        public IInvocation GetCacheInvocation(object proxy, IDictionary<string, object> parameters,
    string serviceId, Type returnType)
        {
            var entry = (from q in _serviceEntryManager.GetAllEntries()
                         let k = q.Attributes
                         where q.Descriptor.Id == serviceId
                         select q).FirstOrDefault();
            var constructor = InvocationMethods.CompositionInvocationConstructor;
            return constructor.Invoke(new object[]{
                    parameters,
                    serviceId,
                    GetKey(parameters),
                    entry.Attributes,
                    returnType,
                    proxy
                }) as IInvocation;
        }

        /// <summary>
        /// The GetInvocation
        /// </summary>
        /// <param name="proxy">The proxy<see cref="object"/></param>
        /// <param name="parameters">The parameters<see cref="IDictionary{string, object}"/></param>
        /// <param name="serviceId">The serviceId<see cref="string"/></param>
        /// <param name="returnType">The returnType<see cref="Type"/></param>
        /// <returns>The <see cref="IInvocation"/></returns>
        public IInvocation GetInvocation(object proxy, IDictionary<string, object> parameters,
            string serviceId, Type returnType)
        {
            var constructor = InvocationMethods.CompositionInvocationConstructor;
            return constructor.Invoke(new object[]{
                    parameters,
                    serviceId,
                    null,
                    null,
                    returnType,
                    proxy
                }) as IInvocation;
        }

        /// <summary>
        /// The GetKey
        /// </summary>
        /// <param name="parameterValue">The parameterValue<see cref="IDictionary{string, object}"/></param>
        /// <returns>The <see cref="string[]"/></returns>
        private string[] GetKey(IDictionary<string, object> parameterValue)
        {
            var param = parameterValue.Values.FirstOrDefault();
            var reuslt = default(string[]);
            if (parameterValue.Count() > 0)
            {
                reuslt = new string[] { param.ToString() };
                if (!(param is IEnumerable))
                {
                    var runtimeProperties = param.GetType().GetRuntimeProperties();
                    var properties = (from q in runtimeProperties
                                      let k = q.GetCustomAttribute<KeyAttribute>()
                                      where k != null
                                      orderby (k as KeyAttribute).SortIndex
                                      select q).ToList();

                    reuslt = properties.Count() > 0 ?
                              properties.Select(p => p.GetValue(parameterValue.Values.FirstOrDefault()).ToString()).ToArray() : reuslt;
                }
            }
            return reuslt;
        }

        /// <summary>
        /// The IsKeyAttributeDerivedType
        /// </summary>
        /// <param name="baseType">The baseType<see cref="Type"/></param>
        /// <param name="derivedType">The derivedType<see cref="Type"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool IsKeyAttributeDerivedType(Type baseType, Type derivedType)
        {
            bool result = false;
            var key = Tuple.Create(baseType, derivedType);
            if (!_derivedTypes.ContainsKey(key))
            {
                result = _derivedTypes.GetOrAdd(key, derivedType.IsSubclassOf(baseType) || derivedType == baseType);
            }
            else
            {
                _derivedTypes.TryGetValue(key, out result);
            }
            return result;
        }

        #endregion 方法
    }
}