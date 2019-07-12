using Autofac;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Surging.Core.ServiceHosting.Internal.Implementation
{
    /// <summary>
    /// Defines the <see cref="ConfigureServicesBuilder" />
    /// </summary>
    public class ConfigureServicesBuilder
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureServicesBuilder"/> class.
        /// </summary>
        /// <param name="configureServices">The configureServices<see cref="MethodInfo"/></param>
        public ConfigureServicesBuilder(MethodInfo configureServices)
        {
            MethodInfo = configureServices;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the MethodInfo
        /// </summary>
        public MethodInfo MethodInfo { get; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The Build
        /// </summary>
        /// <param name="instance">The instance<see cref="object"/></param>
        /// <returns>The <see cref="Func{ContainerBuilder, IContainer}"/></returns>
        public Func<ContainerBuilder, IContainer> Build(object instance) => services => Invoke(instance, services);

        /// <summary>
        /// The Invoke
        /// </summary>
        /// <param name="instance">The instance<see cref="object"/></param>
        /// <param name="services">The services<see cref="ContainerBuilder"/></param>
        /// <returns>The <see cref="IContainer"/></returns>
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

        #endregion 方法
    }
}