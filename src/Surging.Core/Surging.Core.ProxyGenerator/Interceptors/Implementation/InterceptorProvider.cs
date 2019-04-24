using Surging.Core.CPlatform.Runtime.Server;
using System.Collections.Generic;
using System.Linq;
using Surging.Core.CPlatform;
using System.Reflection;
using System.Collections;
using Surging.Core.ProxyGenerator.Utilitys;
using System.Collections.Concurrent;
using System;
using Microsoft.Extensions.Logging;

namespace Surging.Core.ProxyGenerator.Interceptors.Implementation
{
    public class InterceptorProvider : IInterceptorProvider
    {
        private readonly IServiceEntryManager _serviceEntryManager;
        ConcurrentDictionary<Tuple<Type, Type>,bool> _derivedTypes = new ConcurrentDictionary<Tuple<Type, Type>, bool>();
        public InterceptorProvider(IServiceEntryManager serviceEntryManager)
        {
            _serviceEntryManager = serviceEntryManager;
        }
        public IInvocation GetInvocation(object proxy, IDictionary<string, object> parameters,
            string serviceId,Type returnType)
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

        private bool IsKeyAttributeDerivedType(Type baseType,Type derivedType)
        {
            bool result = false;
            var key = Tuple.Create(baseType, derivedType);
            if (!_derivedTypes.ContainsKey(key))
            {
                result =_derivedTypes.GetOrAdd(key, derivedType.IsSubclassOf(baseType) || derivedType == baseType);
            }
            else
            {
                _derivedTypes.TryGetValue(key, out result);
            }
            return result;
        }
    }
}
