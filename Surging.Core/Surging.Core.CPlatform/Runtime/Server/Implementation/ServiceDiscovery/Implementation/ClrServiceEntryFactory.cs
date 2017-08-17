using Microsoft.Extensions.DependencyInjection;
using Surging.Core.CPlatform.Convertibles;
using Surging.Core.CPlatform.Ids;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Implementation
{
    /// <summary>
    /// Clr服务条目工厂。
    /// </summary>
    public class ClrServiceEntryFactory : IClrServiceEntryFactory
    {
        #region Field
        private readonly CPlatformContainer _serviceProvider;
        private readonly IServiceIdGenerator _serviceIdGenerator;
        private readonly ITypeConvertibleService _typeConvertibleService;
        #endregion Field

        #region Constructor
        public ClrServiceEntryFactory(CPlatformContainer serviceProvider, IServiceIdGenerator serviceIdGenerator, ITypeConvertibleService typeConvertibleService)
        {
            _serviceProvider = serviceProvider;
            _serviceIdGenerator = serviceIdGenerator;
            _typeConvertibleService = typeConvertibleService;
        }

        #endregion Constructor

        #region Implementation of IClrServiceEntryFactory

        /// <summary>
        /// 创建服务条目。
        /// </summary>
        /// <param name="service">服务类型。</param>
        /// <param name="serviceImplementation">服务实现类型。</param>
        /// <returns>服务条目集合。</returns>
        public IEnumerable<ServiceEntry> CreateServiceEntry(Type service)
        {
            foreach (var methodInfo in service.GetTypeInfo().GetMethods())
            {
                yield return Create(methodInfo);
            }
        }

        #endregion Implementation of IClrServiceEntryFactory

        #region Private Method

        private ServiceEntry Create(MethodInfo method)
        {
            var serviceId = _serviceIdGenerator.GenerateServiceId(method);

            var serviceDescriptor = new ServiceDescriptor
            {
                Id = serviceId,

            };
            var descriptorAttributes = method.GetCustomAttributes<ServiceDescriptorAttribute>();
            foreach (var descriptorAttribute in descriptorAttributes)
            {
                descriptorAttribute.Apply(serviceDescriptor);
            }
           var fastInvoker = FastInvoke.GetMethodInvoker(method);
            return new ServiceEntry
            {
                Descriptor = serviceDescriptor,
                Attributes = method.GetCustomAttributes().ToList(),
                Func = (key, parameters) =>
             {

                 var instance = _serviceProvider.GetInstances(key, method.DeclaringType);
                 var list = new List<object>();
            
                 foreach (var parameterInfo in method.GetParameters())
                 {
                     var value = parameters[parameterInfo.Name];
                     var parameterType = parameterInfo.ParameterType;
                     var parameter = _typeConvertibleService.Convert(value, parameterType);
                     list.Add(parameter);
                 }
                
                 var result = fastInvoker(instance, list.ToArray()); //method.Invoke(instance, list.ToArray());
                 return Task.FromResult(result);
             }
            };
        }
        #endregion Private Method
    }
}