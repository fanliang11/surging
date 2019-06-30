using Surging.Core.CPlatform.Convertibles;
using Surging.Core.CPlatform.DependencyResolution;
using Surging.Core.CPlatform.Filters.Implementation;
using Surging.Core.CPlatform.Ids;
using Surging.Core.CPlatform.Routing.Template;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static Surging.Core.CPlatform.Utilities.FastInvoke;

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
            var routeTemplate = service.GetCustomAttribute<ServiceBundleAttribute>();
            foreach (var methodInfo in service.GetTypeInfo().GetMethods())
            {
                var serviceRoute = methodInfo.GetCustomAttribute<ServiceRouteAttribute>();
                var routeTemplateVal = routeTemplate.RouteTemplate;
                if (!routeTemplate.IsPrefix && serviceRoute != null)
                     routeTemplateVal = serviceRoute.Template;
                else if (routeTemplate.IsPrefix && serviceRoute != null)
                   routeTemplateVal = $"{ routeTemplate.RouteTemplate}/{ serviceRoute.Template}";
                yield return Create(methodInfo, service.Name, routeTemplateVal);
            }
        }
        #endregion Implementation of IClrServiceEntryFactory

        #region Private Method

        private ServiceEntry Create(MethodInfo method, string serviceName, string routeTemplate)
        {
            var serviceId = _serviceIdGenerator.GenerateServiceId(method);
            var attributes = method.GetCustomAttributes().ToList();
            var serviceDescriptor = new ServiceDescriptor
            {
                Id = serviceId,
                RoutePath = RoutePatternParser.Parse(routeTemplate, serviceName, method.Name)
            };
            var descriptorAttributes = method.GetCustomAttributes<ServiceDescriptorAttribute>();
            foreach (var descriptorAttribute in descriptorAttributes)
            {
                descriptorAttribute.Apply(serviceDescriptor);
            }
            var authorization = attributes.Where(p => p is AuthorizationFilterAttribute).FirstOrDefault();
            if (authorization != null)
                serviceDescriptor.EnableAuthorization(true);
            if (authorization != null)
            {
                serviceDescriptor.AuthType(((authorization as AuthorizationAttribute)?.AuthType)
                    ?? AuthorizationType.AppSecret);
            }
            var fastInvoker = GetHandler(serviceId, method);
            return new ServiceEntry
            {
                Descriptor = serviceDescriptor,
                RoutePath = serviceDescriptor.RoutePath,
                MethodName = method.Name,
                Type = method.DeclaringType,
                Attributes = attributes,
                Func = (key, parameters) =>
             {
                 object instance = null;
                 if (AppConfig.ServerOptions.IsModulePerLifetimeScope)
                     instance = _serviceProvider.GetInstancePerLifetimeScope(key, method.DeclaringType);
                 else
                     instance = _serviceProvider.GetInstances(key, method.DeclaringType);
                 var list = new List<object>();

                 foreach (var parameterInfo in method.GetParameters())
                 {
                     //加入是否有默认值的判断，有默认值，并且用户没传，取默认值
                     if (parameterInfo.HasDefaultValue && !parameters.ContainsKey(parameterInfo.Name))
                     {
                         list.Add(parameterInfo.DefaultValue);
                         continue;
                     }
                     var value = parameters[parameterInfo.Name];
                     var parameterType = parameterInfo.ParameterType;
                     var parameter = _typeConvertibleService.Convert(value, parameterType);
                     list.Add(parameter);
                 }
                 var result = fastInvoker(instance, list.ToArray());
                 return Task.FromResult(result);
             }
            };
        }
        
        private FastInvokeHandler GetHandler(string key, MethodInfo method)
        {
            var objInstance = ServiceResolver.Current.GetService(null, key);
            if (objInstance == null)
            {
                objInstance = FastInvoke.GetMethodInvoker(method);
                ServiceResolver.Current.Register(key, objInstance, null);
            }
            return objInstance as FastInvokeHandler;
        }
        #endregion Private Method
    }
}