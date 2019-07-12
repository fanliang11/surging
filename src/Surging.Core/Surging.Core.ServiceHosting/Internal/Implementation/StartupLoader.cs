// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Surging.Core.ServiceHosting.Startup.Implementation;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Surging.Core.ServiceHosting.Internal.Implementation
{
    /// <summary>
    /// Defines the <see cref="StartupLoader" />
    /// </summary>
    public class StartupLoader
    {
        #region 方法

        /// <summary>
        /// The FindStartupType
        /// </summary>
        /// <param name="startupAssemblyName">The startupAssemblyName<see cref="string"/></param>
        /// <param name="environmentName">The environmentName<see cref="string"/></param>
        /// <returns>The <see cref="Type"/></returns>
        public static Type FindStartupType(string startupAssemblyName, string environmentName)
        {
            if (string.IsNullOrEmpty(startupAssemblyName))
            {
                throw new ArgumentException(
                    string.Format("'{0}' 不能为空.",
                    nameof(startupAssemblyName)),
                    nameof(startupAssemblyName));
            }

            var assembly = Assembly.Load(new AssemblyName(startupAssemblyName));
            if (assembly == null)
            {
                throw new InvalidOperationException(String.Format("程序集 '{0}' 错误不能加载", startupAssemblyName));
            }

            var startupNameWithEnv = "Startup" + environmentName;
            var startupNameWithoutEnv = "Startup";
            var type =
                assembly.GetType(startupNameWithEnv) ??
                assembly.GetType(startupAssemblyName + "." + startupNameWithEnv) ??
                assembly.GetType(startupNameWithoutEnv) ??
                assembly.GetType(startupAssemblyName + "." + startupNameWithoutEnv);

            if (type == null)
            {
                var definedTypes = assembly.DefinedTypes.ToList();
                var startupType1 = definedTypes.Where(info => info.Name.Equals(startupNameWithEnv, StringComparison.OrdinalIgnoreCase));
                var startupType2 = definedTypes.Where(info => info.Name.Equals(startupNameWithoutEnv, StringComparison.OrdinalIgnoreCase));
                var typeInfo = startupType1.Concat(startupType2).FirstOrDefault();
                if (typeInfo != null)
                {
                    type = typeInfo.AsType();
                }
            }

            if (type == null)
            {
                throw new InvalidOperationException(String.Format("类型 '{0}' 或者 '{1}' 不能从程序集 '{2}'找到.",
                    startupNameWithEnv,
                    startupNameWithoutEnv,
                    startupAssemblyName));
            }

            return type;
        }

        /// <summary>
        /// The LoadMethods
        /// </summary>
        /// <param name="hostingServiceProvider">The hostingServiceProvider<see cref="IServiceProvider"/></param>
        /// <param name="config">The config<see cref="IConfigurationBuilder"/></param>
        /// <param name="startupType">The startupType<see cref="Type"/></param>
        /// <param name="environmentName">The environmentName<see cref="string"/></param>
        /// <returns>The <see cref="StartupMethods"/></returns>
        public static StartupMethods LoadMethods(IServiceProvider hostingServiceProvider, IConfigurationBuilder config, Type startupType, string environmentName)
        {
            var configureMethod = FindConfigureDelegate(startupType, environmentName);
            var servicesMethod = FindConfigureServicesDelegate(startupType, environmentName);
            var configureContainerMethod = FindConfigureContainerDelegate(startupType, environmentName);

            object instance = null;
            if (!configureMethod.MethodInfo.IsStatic || (servicesMethod != null && !servicesMethod.MethodInfo.IsStatic))
            {
                instance = ActivatorUtilities.CreateInstance(hostingServiceProvider, startupType, config);
            }

            var configureServicesCallback = servicesMethod.Build(instance);
            var configureContainerCallback = configureContainerMethod.Build(instance);

            Func<ContainerBuilder, IContainer> configureServices = services =>
            {
                IContainer applicationServiceProvider = configureServicesCallback.Invoke(services);
                if (applicationServiceProvider != null)
                {
                    return applicationServiceProvider;
                }
                if (configureContainerMethod.MethodInfo != null)
                {
                    var serviceProviderFactoryType = typeof(IServiceProviderFactory<>).MakeGenericType(configureContainerMethod.GetContainerType());
                    var serviceProviderFactory = hostingServiceProvider.GetRequiredService(serviceProviderFactoryType);
                    var builder = serviceProviderFactoryType.GetMethod(nameof(DefaultServiceProviderFactory.CreateBuilder)).Invoke(serviceProviderFactory, new object[] { services });
                    configureContainerCallback.Invoke(builder);
                    applicationServiceProvider = (IContainer)serviceProviderFactoryType.GetMethod(nameof(DefaultServiceProviderFactory.CreateServiceProvider)).Invoke(serviceProviderFactory, new object[] { builder });
                }

                return applicationServiceProvider;
            };

            return new StartupMethods(instance, configureMethod.Build(instance), configureServices);
        }

        /// <summary>
        /// The FindConfigureContainerDelegate
        /// </summary>
        /// <param name="startupType">The startupType<see cref="Type"/></param>
        /// <param name="environmentName">The environmentName<see cref="string"/></param>
        /// <returns>The <see cref="ConfigureContainerBuilder"/></returns>
        private static ConfigureContainerBuilder FindConfigureContainerDelegate(Type startupType, string environmentName)
        {
            var configureMethod = FindMethod(startupType, "Configure{0}Container", environmentName, typeof(void), required: false);
            return new ConfigureContainerBuilder(configureMethod);
        }

        /// <summary>
        /// The FindConfigureDelegate
        /// </summary>
        /// <param name="startupType">The startupType<see cref="Type"/></param>
        /// <param name="environmentName">The environmentName<see cref="string"/></param>
        /// <returns>The <see cref="ConfigureBuilder"/></returns>
        private static ConfigureBuilder FindConfigureDelegate(Type startupType, string environmentName)
        {
            var configureMethod = FindMethod(startupType, "Configure{0}", environmentName, typeof(void), required: true);
            return new ConfigureBuilder(configureMethod);
        }

        /// <summary>
        /// The FindConfigureServicesDelegate
        /// </summary>
        /// <param name="startupType">The startupType<see cref="Type"/></param>
        /// <param name="environmentName">The environmentName<see cref="string"/></param>
        /// <returns>The <see cref="ConfigureServicesBuilder"/></returns>
        private static ConfigureServicesBuilder FindConfigureServicesDelegate(Type startupType, string environmentName)
        {
            var servicesMethod = FindMethod(startupType, "Configure{0}Services", environmentName, typeof(IContainer), required: false)
                ?? FindMethod(startupType, "Configure{0}Services", environmentName, typeof(void), required: false);
            return new ConfigureServicesBuilder(servicesMethod);
        }

        /// <summary>
        /// The FindMethod
        /// </summary>
        /// <param name="startupType">The startupType<see cref="Type"/></param>
        /// <param name="methodName">The methodName<see cref="string"/></param>
        /// <param name="environmentName">The environmentName<see cref="string"/></param>
        /// <param name="returnType">The returnType<see cref="Type"/></param>
        /// <param name="required">The required<see cref="bool"/></param>
        /// <returns>The <see cref="MethodInfo"/></returns>
        private static MethodInfo FindMethod(Type startupType, string methodName, string environmentName, Type returnType = null, bool required = true)
        {
            var methodNameWithEnv = string.Format(CultureInfo.InvariantCulture, methodName, environmentName);
            var methodNameWithNoEnv = string.Format(CultureInfo.InvariantCulture, methodName, "");

            var methods = startupType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            var selectedMethods = methods.Where(method => method.Name.Equals(methodNameWithEnv, StringComparison.OrdinalIgnoreCase)).ToList();
            if (selectedMethods.Count > 1)
            {
                throw new InvalidOperationException(string.Format("多个重载方法  '{0}' 不支持.", methodNameWithEnv));
            }
            if (selectedMethods.Count == 0)
            {
                selectedMethods = methods.Where(method => method.Name.Equals(methodNameWithNoEnv, StringComparison.OrdinalIgnoreCase)).ToList();
                if (selectedMethods.Count > 1)
                {
                    throw new InvalidOperationException(string.Format("多个重载方法  '{0}' 不支持.", methodNameWithNoEnv));
                }
            }

            var methodInfo = selectedMethods.FirstOrDefault();
            if (methodInfo == null)
            {
                if (required)
                {
                    throw new InvalidOperationException(string.Format("公共方法名称必须为'{0}' 或者 '{1}' 找不到 '{2}' 类型.",
                        methodNameWithEnv,
                        methodNameWithNoEnv,
                        startupType.FullName));
                }
                return null;
            }
            if (returnType != null && methodInfo.ReturnType != returnType)
            {
                if (required)
                {
                    throw new InvalidOperationException(string.Format(" '{0}'的方法在类型 '{1}' 必须有返回类型 '{2}'.",
                        methodInfo.Name,
                        startupType.FullName,
                        returnType.Name));
                }
                return null;
            }
            return methodInfo;
        }

        #endregion 方法
    }
}