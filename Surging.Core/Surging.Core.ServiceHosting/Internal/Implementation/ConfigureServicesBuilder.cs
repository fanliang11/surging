using Autofac;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Surging.Core.ServiceHosting.Internal.Implementation
{
   public  class ConfigureServicesBuilder
    {
        public ConfigureServicesBuilder(MethodInfo configureServices)
        {
            MethodInfo = configureServices;
        }

        public MethodInfo MethodInfo { get; }

        public Func<ContainerBuilder, IContainer> Build(object instance) => services => Invoke(instance, services);

        private IContainer Invoke(object instance, ContainerBuilder services)
        {
            if (MethodInfo == null)
            {
                return null;
            }

            //  只支持ContainerBuilder参数
            var parameters = MethodInfo.GetParameters();
            if (parameters.Length > 1 ||
                parameters.Any(p => p.ParameterType != typeof(ContainerBuilder)))
            {
                throw new InvalidOperationException("configureservices方法必须是无参数或只有一个参数为ContainerBuilder类型");
            }

            var arguments = new object[MethodInfo.GetParameters().Length];

            if (parameters.Length > 0)
            {
                arguments[0] = services;
            }

            return MethodInfo.Invoke(instance, arguments) as IContainer;
        }
    }
}