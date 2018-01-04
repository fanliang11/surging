using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Convertibles;
using Surging.Core.CPlatform.Convertibles.Implementation;
using Surging.Core.CPlatform.EventBus.Events;
using Surging.Core.CPlatform.Filters;
using Surging.Core.CPlatform.Filters.Implementation;
using Surging.Core.CPlatform.Ids;
using Surging.Core.CPlatform.Ids.Implementation;
using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Routing.Implementation;
using Surging.Core.CPlatform.Runtime.Client;
using Surging.Core.CPlatform.Runtime.Client.Address.Resolvers;
using Surging.Core.CPlatform.Runtime.Client.Address.Resolvers.Implementation;
using Surging.Core.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors;
using Surging.Core.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors.Implementation;
using Surging.Core.CPlatform.Runtime.Client.HealthChecks;
using Surging.Core.CPlatform.Runtime.Client.HealthChecks.Implementation;
using Surging.Core.CPlatform.Runtime.Client.Implementation;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Runtime.Server.Implementation;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Implementation;
using Surging.Core.CPlatform.Serialization;
using Surging.Core.CPlatform.Serialization.Implementation;
using Surging.Core.CPlatform.Support;
using Surging.Core.CPlatform.Support.Implementation;
using Surging.Core.CPlatform.Transport.Codec;
using Surging.Core.CPlatform.Transport.Codec.Implementation;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Surging.Core.CPlatform
{
    /// <summary>
    /// 服务构建者。
    /// </summary>
    public interface IServiceBuilder
    {
        /// <summary>
        /// 服务集合。
        /// </summary>
        ContainerBuilder Services { get; set; }
    }

    /// <summary>
    /// 默认服务构建者。
    /// </summary>
    internal sealed class ServiceBuilder : IServiceBuilder
    {
        public ServiceBuilder(ContainerBuilder services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            Services = services;
        }

        #region Implementation of IServiceBuilder

        /// <summary>
        /// 服务集合。
        /// </summary>
        public ContainerBuilder Services { get; set; }

        #endregion Implementation of IServiceBuilder
    }

    public static class ContainerBuilderExtensions
    {
        private static List<Assembly> _referenceAssembly = new List<Assembly>();

        /// <summary>
        /// 添加Json序列化支持。
        /// </summary>
        /// <param name="builder">服务构建者。</param>
        /// <returns>服务构建者。</returns>
        public static IServiceBuilder AddJsonSerialization(this IServiceBuilder builder)
        {
            var services = builder.Services;
            builder.Services.RegisterType(typeof(JsonSerializer)).As(typeof(ISerializer<string>)).SingleInstance();
            builder.Services.RegisterType(typeof(StringByteArraySerializer)).As(typeof(ISerializer<byte[]>)).SingleInstance();
            builder.Services.RegisterType(typeof(StringObjectSerializer)).As(typeof(ISerializer<object>)).SingleInstance();
            return builder;
        }
        #region RouteManager

        /// <summary>
        /// 设置服务路由管理者。
        /// </summary>
        /// <typeparam name="T">服务路由管理者实现。</typeparam>
        /// <param name="builder">服务构建者。</param>
        /// <returns>服务构建者。</returns>
        public static IServiceBuilder UseRouteManager<T>(this IServiceBuilder builder) where T : class, IServiceRouteManager
        {
            builder.Services.RegisterType(typeof(T)).As(typeof(IServiceRouteManager)).SingleInstance();
            return builder;
        }

        /// <summary>
        /// 设置服务路由管理者。
        /// </summary>
        /// <param name="builder">服务构建者。</param>
        /// <param name="factory">服务路由管理者实例工厂。</param>
        /// <returns>服务构建者。</returns>
        public static IServiceBuilder UseRouteManager(this IServiceBuilder builder, Func<IServiceProvider, IServiceRouteManager> factory)
        {
            builder.Services.RegisterAdapter(factory).InstancePerLifetimeScope();
            return builder;
        }

        /// <summary>
        /// 设置服务订阅管理者。
        /// </summary>
        /// <param name="builder">服务构建者。</param>
        /// <param name="factory">服务订阅管理者实例工厂。</param>
        /// <returns>服务构建者。</returns>
        public static IServiceBuilder UseSubscribeManager(this IServiceBuilder builder, Func<IServiceProvider, IServiceSubscribeManager> factory)
        {
            builder.Services.RegisterAdapter(factory).InstancePerLifetimeScope();
            return builder;
        }

        /// <summary>
        /// 设置服务命令管理者。
        /// </summary>
        /// <param name="builder">服务构建者。</param>
        /// <param name="factory">服务命令管理者实例工厂。</param>
        /// <returns>服务构建者。</returns>
        public static IServiceBuilder UseCommandManager(this IServiceBuilder builder, Func<IServiceProvider, IServiceCommandManager> factory)
        {
            builder.Services.RegisterAdapter(factory).InstancePerLifetimeScope();
            return builder;
        }

        /// <summary>
        /// 设置服务路由管理者。
        /// </summary>
        /// <param name="builder">服务构建者。</param>
        /// <param name="instance">服务路由管理者实例。</param>
        /// <returns>服务构建者。</returns>
        public static IServiceBuilder UseRouteManager(this IServiceBuilder builder, IServiceRouteManager instance)
        {
            builder.Services.RegisterInstance(instance);
            return builder;
        }

        #endregion RouteManager

        /// <summary>
        /// 设置共享文件路由管理者。
        /// </summary>
        /// <param name="builder">服务构建者。</param>
        /// <param name="filePath">文件路径。</param>
        /// <returns>服务构建者。</returns>
        public static IServiceBuilder UseSharedFileRouteManager(this IServiceBuilder builder, string filePath)
        {
            return builder.UseRouteManager(provider =>
            new SharedFileServiceRouteManager(
                filePath,
                provider.GetRequiredService<ISerializer<string>>(),
                provider.GetRequiredService<IServiceRouteFactory>(),
                provider.GetRequiredService<ILogger<SharedFileServiceRouteManager>>()));
        }

        public static IServiceBuilder UseSharedFileRouteManager(this IServiceBuilder builder, string ip, string port)
        {
            return builder.UseRouteManager(provider =>
            new SharedFileServiceRouteManager(
                ip,
                provider.GetRequiredService<ISerializer<string>>(),
                provider.GetRequiredService<IServiceRouteFactory>(),
                provider.GetRequiredService<ILogger<SharedFileServiceRouteManager>>()));
        }

        #region AddressSelector
        /// <summary>
        /// 使用轮询的地址选择器。
        /// </summary>
        /// <param name="builder">服务构建者。</param>
        /// <returns>服务构建者。</returns>
        public static IServiceBuilder UsePollingAddressSelector(this IServiceBuilder builder)
        {
            builder.Services.RegisterType(typeof(PollingAddressSelector))
                .Named(AddressSelectorMode.Polling.ToString(), typeof(IAddressSelector));
            return builder;
        }

        /// <summary>
        /// 使用哈希的地址选择器。
        /// </summary>
        /// <param name="builder">服务构建者。</param>
        /// <returns>服务构建者。</returns>
        public static IServiceBuilder UseHashAlgorithmAddressSelector(this IServiceBuilder builder)
        {
            builder.Services.RegisterType(typeof(HashAlgorithmAdrSelector))
                .Named(AddressSelectorMode.HashAlgorithm.ToString(), typeof(IAddressSelector)).SingleInstance();
            return builder;
        }

        /// <summary>
        /// 使用随机的地址选择器。
        /// </summary>
        /// <param name="builder">服务构建者。</param>
        /// <returns>服务构建者。</returns>
        public static IServiceBuilder UseRandomAddressSelector(this IServiceBuilder builder)
        {
            builder.Services.RegisterType(typeof(RandomAddressSelector))
                .Named(AddressSelectorMode.Random.ToString(), typeof(IAddressSelector)).SingleInstance();
            return builder;
        }

        /// <summary>
        /// 设置服务地址选择器。
        /// </summary>
        /// <param name="builder">服务构建者。</param>
        /// <param name="instance">地址选择器实例。</param>
        /// <returns>服务构建者。</returns>
        public static IServiceBuilder UseAddressSelector(this IServiceBuilder builder)
        {
            return builder.UseRandomAddressSelector().UsePollingAddressSelector().UseHashAlgorithmAddressSelector();
        }

        #endregion AddressSelector

        #region Codec Factory

        /// <summary>
        /// 使用编解码器。
        /// </summary>
        /// <param name="builder">服务构建者。</param>
        /// <param name="codecFactory"></param>
        /// <returns>服务构建者。</returns>
        public static IServiceBuilder UseCodec(this IServiceBuilder builder, ITransportMessageCodecFactory codecFactory)
        {
            builder.Services.RegisterInstance(codecFactory);

            return builder;
        }

        /// <summary>
        /// 使用编解码器。
        /// </summary>
        /// <typeparam name="T">编解码器工厂实现类型。</typeparam>
        /// <param name="builder">服务构建者。</param>
        /// <returns>服务构建者。</returns>
        public static IServiceBuilder UseCodec<T>(this IServiceBuilder builder) where T : class, ITransportMessageCodecFactory
        {
            builder.Services.RegisterType(typeof(T)).As(typeof(ITransportMessageCodecFactory)).SingleInstance();
            return builder;
        }

        /// <summary>
        /// 使用编解码器。
        /// </summary>
        /// <param name="builder">服务构建者。</param>
        /// <param name="codecFactory">编解码器工厂创建委托。</param>
        /// <returns>服务构建者。</returns>
        public static IServiceBuilder UseCodec(this IServiceBuilder builder, Func<IServiceProvider, ITransportMessageCodecFactory> codecFactory)
        {
            builder.Services.RegisterAdapter(codecFactory);
            return builder;
        }

        #endregion Codec Factory

        /// <summary>
        /// 使用Json编解码器。
        /// </summary>
        /// <param name="builder">服务构建者。</param>
        /// <returns>服务构建者。</returns>
        public static IServiceBuilder UseJsonCodec(this IServiceBuilder builder)
        {
            return builder.UseCodec<JsonTransportMessageCodecFactory>();
        }

        /// <summary>
        /// 添加客户端运行时服务。
        /// </summary>
        /// <param name="builder">服务构建者。</param>
        /// <returns>服务构建者。</returns>
        public static IServiceBuilder AddClientRuntime(this IServiceBuilder builder)
        {
            var services = builder.Services;
            builder.Services.RegisterType(typeof(DefaultHealthCheckService)).As(typeof(IHealthCheckService)).SingleInstance();
            builder.Services.RegisterType(typeof(DefaultAddressResolver)).As(typeof(IAddressResolver)).SingleInstance();
            builder.Services.RegisterType(typeof(RemoteInvokeService)).As(typeof(IRemoteInvokeService)).SingleInstance();
            return builder.UseAddressSelector().AddRuntime().AddClusterSupport();
        }

        /// <summary>
        /// 添加集群支持
        /// </summary>
        /// <param name="builder">服务构建者</param>
        /// <returns>服务构建者。</returns>
        public static IServiceBuilder AddClusterSupport(this IServiceBuilder builder)
        {

            builder.Services.RegisterType(typeof(ServiceCommandProvider)).As(typeof(IServiceCommandProvider)).SingleInstance();
            builder.Services.RegisterType(typeof(BreakeRemoteInvokeService)).As(typeof(IBreakeRemoteInvokeService)).SingleInstance();
            builder.Services.RegisterType(typeof(FailoverInjectionInvoker)).AsImplementedInterfaces()
                .Named(StrategyType.Injection.ToString(), typeof(IClusterInvoker)).SingleInstance();
            builder.Services.RegisterType(typeof(FailoverHandoverInvoker)).AsImplementedInterfaces()
            .Named(StrategyType.Failover.ToString(), typeof(IClusterInvoker)).SingleInstance();
            return builder;
        }

        /// <summary>
        /// 添加服务运行时服务。
        /// </summary>
        /// <param name="builder">服务构建者。</param>
        /// <returns>服务构建者。</returns>
        public static IServiceBuilder AddServiceRuntime(this IServiceBuilder builder)
        {
            builder.Services.RegisterType(typeof(DefaultServiceEntryLocate)).As(typeof(IServiceEntryLocate)).SingleInstance();
            builder.Services.RegisterType(typeof(DefaultServiceExecutor)).As(typeof(IServiceExecutor)).SingleInstance();
            return builder.RegisterServices().RegisterRepositories().RegisterServiceBus().AddRuntime();
        }

        /// <summary>
        /// 添加关联服务运行时 
        /// </summary>
        /// <param name="builder">服务构建者。</param>
        /// <returns>服务构建者。</returns>
        public static IServiceBuilder AddRelateServiceRuntime(this IServiceBuilder builder)
        {
            var services = builder.Services;
            builder.Services.RegisterType(typeof(DefaultHealthCheckService)).As(typeof(IHealthCheckService)).SingleInstance();
            builder.Services.RegisterType(typeof(DefaultAddressResolver)).As(typeof(IAddressResolver)).SingleInstance();
            builder.Services.RegisterType(typeof(RemoteInvokeService)).As(typeof(IRemoteInvokeService)).SingleInstance();
            return builder.UseAddressSelector().AddClusterSupport();
        }

        /// <summary>
        /// 添加核心服务。
        /// </summary>
        /// <param name="services">服务集合。</param>
        /// <returns>服务构建者。</returns>
        public static IServiceBuilder AddCoreService(this ContainerBuilder services)
        {
            Check.NotNull(services, "services");
            services.RegisterType<DefaultServiceIdGenerator>().As<IServiceIdGenerator>().SingleInstance();
            services.Register(p => new CPlatformContainer(p));
            services.RegisterType(typeof(DefaultTypeConvertibleProvider)).As(typeof(ITypeConvertibleProvider)).SingleInstance();
            services.RegisterType(typeof(DefaultTypeConvertibleService)).As(typeof(ITypeConvertibleService)).SingleInstance();
            services.RegisterType(typeof(AuthorizationAttribute)).As(typeof(IAuthorizationFilter)).SingleInstance();
            services.RegisterType(typeof(AuthorizationAttribute)).As(typeof(IFilter)).SingleInstance();
            services.RegisterType(typeof(DefaultServiceRouteProvider)).As(typeof(IServiceRouteProvider)).SingleInstance();
            services.RegisterType(typeof(DefaultServiceRouteFactory)).As(typeof(IServiceRouteFactory)).SingleInstance();
            services.RegisterType(typeof(DefaultServiceSubscriberFactory)).As(typeof(IServiceSubscriberFactory)).SingleInstance();
            return new ServiceBuilder(services)
                .AddJsonSerialization()
                .UseJsonCodec();

        }

        private static IServiceBuilder AddRuntime(this IServiceBuilder builder)
        {
            var services = builder.Services;

            builder.Services.RegisterType(typeof(ClrServiceEntryFactory)).As(typeof(IClrServiceEntryFactory)).SingleInstance();

            builder.Services.Register(provider =>
            {
                try
                {
                    var assemblys = GetReferenceAssembly();
                    var types = assemblys.SelectMany(i => i.ExportedTypes).ToArray();
                    return new AttributeServiceEntryProvider(types, provider.Resolve<IClrServiceEntryFactory>(),
                         provider.Resolve<ILogger<AttributeServiceEntryProvider>>(),provider.Resolve<CPlatformContainer>());
                }
                finally
                {
                    _referenceAssembly.Clear();
                }
            }).As<IServiceEntryProvider>();
            builder.Services.RegisterType(typeof(DefaultServiceEntryManager)).As(typeof(IServiceEntryManager)).SingleInstance();
            return builder;
        }

        public static void AddMicroService(this ContainerBuilder builder, Action<IServiceBuilder> option)
        {
            option.Invoke(builder.AddCoreService());
        }

        /// <summary>.
        /// 依赖注入业务模块程序集
        /// </summary>
        /// <param name="builder">ioc容器</param>
        /// <returns>返回注册模块信息</returns>
        public static IServiceBuilder RegisterServices(this IServiceBuilder builder)
        {
            var services = builder.Services;
            var referenceAssemblies = GetReferenceAssembly();
            foreach (var assembly in referenceAssemblies)
            {
                services.RegisterAssemblyTypes(assembly)
                   .Where(t => typeof(IServiceKey).GetTypeInfo().IsAssignableFrom(t))
                   .AsImplementedInterfaces();
                services.RegisterAssemblyTypes(assembly)
             .Where(t => typeof(ServiceBase).GetTypeInfo().IsAssignableFrom(t) && t.GetTypeInfo().GetCustomAttribute<ModuleNameAttribute>() == null).AsImplementedInterfaces();

                var types = assembly.GetTypes().Where(t => typeof(ServiceBase).GetTypeInfo().IsAssignableFrom(t) && t.GetTypeInfo().GetCustomAttribute<ModuleNameAttribute>() != null);
                foreach (var type in types)
                {
                    var module = type.GetTypeInfo().GetCustomAttribute<ModuleNameAttribute>();
                    var interfaceObj = type.GetInterfaces()
                        .FirstOrDefault(t => typeof(IServiceKey).GetTypeInfo().IsAssignableFrom(t));
                    if (interfaceObj != null)
                    {
                        services.RegisterType(type).AsImplementedInterfaces().Named(module.ModuleName, interfaceObj);
                        services.RegisterType(type).Named(module.ModuleName, type);
                    }
                }

            }
            return builder;
        }

        public static IServiceBuilder RegisterServiceBus
            (this IServiceBuilder builder)
        {
            var services = builder.Services;
            var referenceAssemblies = GetReferenceAssembly();

            foreach (var assembly in referenceAssemblies)
            {
                services.RegisterAssemblyTypes(assembly)
                 .Where(t => typeof(IIntegrationEventHandler).GetTypeInfo().IsAssignableFrom(t)).AsImplementedInterfaces().SingleInstance();
                services.RegisterAssemblyTypes(assembly)
             .Where(t => typeof(IIntegrationEventHandler).IsAssignableFrom(t)).SingleInstance();
            }
            return builder;
        }

        /// <summary>
        ///依赖注入仓储模块程序集
        /// </summary>
        /// <param name="builder">IOC容器</param>
        /// <returns>返回注册模块信息</returns>
        public static IServiceBuilder RegisterRepositories
         (this IServiceBuilder builder)
        {
            var services = builder.Services;
            var referenceAssemblies = GetReferenceAssembly();

            foreach (var assembly in referenceAssemblies)
            {
                services.RegisterAssemblyTypes(assembly)
                    .Where(t => typeof(BaseRepository).GetTypeInfo().IsAssignableFrom(t));
            }
            return builder;
        }

        public static List<Type> GetInterfaceService(this IServiceBuilder builder)
        {
            var types = new List<Type>();
            var referenceAssemblies = GetReferenceAssembly();
            referenceAssemblies.ForEach(p =>
            {
                types.AddRange(p.GetTypes().Where(t => typeof(IServiceKey).GetTypeInfo().IsAssignableFrom(t)));
            });
            return types;
        }

        private static List<Assembly> GetReferenceAssembly()
        {
            string path = AppContext.BaseDirectory;
            var result = _referenceAssembly;
            if (!result.Any())
            {
                var assemblyNames = GetAllAssemblyFiles(path);
                foreach (var referencedAssemblyName in assemblyNames)
                {
                    var referencedAssembly = Assembly.Load(new AssemblyName(referencedAssemblyName));
                    _referenceAssembly.Add(referencedAssembly);
                }
                result = _referenceAssembly;
            }
            return result;
        }

        private static List<string> GetAllAssemblyFiles(string parentDir)
        { 
                Regex regex = new Regex("Microsoft.\\w*|System.\\w*|Netty.\\w*|Autofac.\\w*|Surging.Core.\\w*", RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
                return
                    Directory.GetFiles(parentDir, "*.dll").Select(Path.GetFileNameWithoutExtension).Where(
                        a => !regex.IsMatch(a)).ToList();
        }
    }
}