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
using System.Runtime.CompilerServices;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Exceptions;
using System.Collections.Concurrent;
using Surging.Core.CPlatform.Ioc;

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
                    httpMethod.AppendJoin(',',attribute.HttpMethods).Append(",");
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
                Methods=httpMethods,
                MethodName = method.Name,
                Parameters = method.GetParameters(),
                Type = method.DeclaringType,
                Attributes = attributes,
                Func = async (key, parameters) =>
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
                     else if(parameterInfo.ParameterType == typeof(CancellationToken))
                     {
                         list.Add(new CancellationToken());
                         continue;
                     }
                     var value = parameters[parameterInfo.Name];

                     if(methodValidateAttribute !=null)
                     _validationProcessor.Validate(parameterInfo, value);

                     var parameterType = parameterInfo.ParameterType;
                     var parameter = _typeConvertibleService.Convert(value, parameterType);
                     list.Add(parameter);
                 }
                 if (!isReactive)
                 {
                     var result = fastInvoker(instance, list.ToArray());
                     return await Task.FromResult(result);
                 }
                 else
                 {
                     var serviceBehavior = instance as IServiceBehavior;
                     var callbackTask = RegisterResultCallbackAsync(serviceBehavior.MessageId, Task.Factory.CancellationToken);
                     serviceBehavior.Received += MessageListener_Received;
                     var result = fastInvoker(instance, list.ToArray());
                     return await callbackTask;
                 }
             }
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<object> RegisterResultCallbackAsync(string id, CancellationToken cancellationToken)
        {

            var task = new ManualResetValueTaskSource<TransportMessage>();
            _resultDictionary.TryAdd(id, task);
            try
            {
                var result = await task.AwaitValue(cancellationToken);
                return result.GetContent<ReactiveResultMessage>()?.Result;
            }
            finally
            {
                //ɾ���ص�����
                ManualResetValueTaskSource<TransportMessage> value;
                _resultDictionary.TryRemove(id, out value);
                value.SetCanceled();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task MessageListener_Received(TransportMessage message)
        {
            ManualResetValueTaskSource<TransportMessage> task;
            if (!_resultDictionary.TryGetValue(message.Id, out task))
                return;

            if (message.IsReactiveMessage())
            {
                var content = message.GetContent<ReactiveResultMessage>();
                if (!string.IsNullOrEmpty(content.ExceptionMessage))
                {
                    task.SetException(new CPlatformCommunicationException(content.ExceptionMessage, content.StatusCode));
                }
                else
                {
                    task.SetResult(message);
                }
            }
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