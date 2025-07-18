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
using System.Text;
using System.Threading.Tasks;
using Surging.Core.CPlatform.Validation;
using static Surging.Core.CPlatform.Utilities.FastInvoke;
using System.Threading;
using System.Collections.Concurrent;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Ioc;
using System.Runtime.CompilerServices;
using Surging.Core.CPlatform.Exceptions;

namespace Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Implementation
{
    /// <summary>
    /// Clr������Ŀ������
    /// </summary>
    public class ClrServiceEntryFactory : IClrServiceEntryFactory
    {
        #region Field
        private readonly CPlatformContainer _serviceProvider;
        private readonly IServiceIdGenerator _serviceIdGenerator;
        private readonly ITypeConvertibleService _typeConvertibleService;
        private readonly IValidationProcessor _validationProcessor;
        private readonly ConcurrentDictionary<string, ManualResetValueTaskSource<TransportMessage>> _resultDictionary =
          new ConcurrentDictionary<string, ManualResetValueTaskSource<TransportMessage>>();
        #endregion Field

        #region Constructor
        public ClrServiceEntryFactory(CPlatformContainer serviceProvider, IServiceIdGenerator serviceIdGenerator, ITypeConvertibleService typeConvertibleService, IValidationProcessor validationProcessor)
        {
            _serviceProvider = serviceProvider;
            _serviceIdGenerator = serviceIdGenerator;
            _typeConvertibleService = typeConvertibleService;
            _validationProcessor = validationProcessor;
        }

        #endregion Constructor

        #region Implementation of IClrServiceEntryFactory

        /// <summary>
        /// ����������Ŀ��
        /// </summary>
        /// <param name="service">�������͡�</param>
        /// <param name="serviceImplementation">����ʵ�����͡�</param>
        /// <returns>������Ŀ���ϡ�</returns>
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
            var httpMethodAttributes = attributes.Where(p => p is HttpMethodAttribute).Select(p => p as HttpMethodAttribute).ToList();
            var httpMethods = new List<string>();
            StringBuilder httpMethod = new StringBuilder();
            foreach (var attribute in httpMethodAttributes)
            {
                httpMethods.AddRange(attribute.HttpMethods);
                if (attribute.IsRegisterMetadata)
                    httpMethod.AppendJoin(',', attribute.HttpMethods).Append(",");
            }
            if (httpMethod.Length > 0)
            {
                httpMethod.Length = httpMethod.Length - 1;
                serviceDescriptor.HttpMethod(httpMethod.ToString());
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

            var methodValidateAttribute = attributes.Where(p => p is ValidateAttribute)
                .Cast<ValidateAttribute>().FirstOrDefault();
            var isReactive = attributes.Any(p => p is ReactiveAttribute);
            return new ServiceEntry
            {
                Descriptor = serviceDescriptor,
                RoutePath = serviceDescriptor.RoutePath,
                Methods = httpMethods,
                MethodName = method.Name,
                Type = method.DeclaringType,
                Parameters = method.GetParameters(),
                IsPermission = method.DeclaringType.GetCustomAttribute<BaseActionFilterAttribute>() != null || attributes.Any(p => p is BaseActionFilterAttribute),
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
                     //�����Ƿ���Ĭ��ֵ���жϣ���Ĭ��ֵ�������û�û����ȡĬ��ֵ
                     if (parameterInfo.HasDefaultValue && !parameters.ContainsKey(parameterInfo.Name))
                     {
                         list.Add(parameterInfo.DefaultValue);
                         continue;
                     }
                     else if (parameterInfo.ParameterType == typeof(CancellationToken))
                     {
                         list.Add(new CancellationToken());
                         continue;
                     }
                     var value = parameters[parameterInfo.Name];

                     if (methodValidateAttribute != null)
                         _validationProcessor.Validate(parameterInfo, value);

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