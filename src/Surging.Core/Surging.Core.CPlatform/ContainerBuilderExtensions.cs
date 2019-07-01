using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Cache;
using Surging.Core.CPlatform.Configurations;
using Surging.Core.CPlatform.Configurations.Watch;
using Surging.Core.CPlatform.Convertibles;
using Surging.Core.CPlatform.Convertibles.Implementation;
using Surging.Core.CPlatform.Engines;
using Surging.Core.CPlatform.Engines.Implementation;
using Surging.Core.CPlatform.EventBus.Events;
using Surging.Core.CPlatform.Filters;
using Surging.Core.CPlatform.Filters.Implementation;
using Surging.Core.CPlatform.HashAlgorithms;
using Surging.Core.CPlatform.Ids;
using Surging.Core.CPlatform.Ids.Implementation;
using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Module;
using Surging.Core.CPlatform.Mqtt;
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
using System.Runtime.Serialization;
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
        private static List<AbstractModule> _modules = new List<AbstractModule>();

        /// <summary>
        /// 添加Json序列化支持。
        /// </summary>
        /// <param name="builder">服务构建者。</param>
        /// <returns>服务构建者。</returns>
        public static IServiceBuilder AddJsonSerialization(this IServiceBuilder builder)
        {
            var services = builder.Services;
            services.RegisterType(typeof(JsonSerializer)).As(typeof(ISerializer<string>)).SingleInstance();
            services.RegisterType(typeof(StringByteArraySerializer)).As(typeof(ISerializer<byte[]>)).SingleInstance();
            services.RegisterType(typeof(StringObjectSerializer)).As(typeof(ISerializer<object>)).SingleInstance();
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
        /// 设置缓存管理者。
        /// </summary>
        /// <param name="builder">服务构建者。</param>
        /// <param name="factory">缓存管理者实例工厂。</param>
        /// <returns>服务构建者。</returns>
        public static IServiceBuilder UseCacheManager(this IServiceBuilder builder, Func<IServiceProvider, IServiceCacheManager> factory)
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

        /// <summary>
        /// 设置mqtt服务路由管理者。
        /// </summary>
        /// <param name="builder">mqtt服务构建者。</param>
        /// <param name="factory">mqtt服务路由管理者实例工厂。</param>
        /// <returns>服务构建者。</returns>
        public static IServiceBuilder UseMqttRouteManager(this IServiceBuilder builder, Func<IServiceProvider, IMqttServiceRouteManager> factory)
        {
            builder.Services.RegisterAdapter(factory).InstancePerLifetimeScope();
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
                .Named(AddressSelectorMode.Polling.ToString(), typeof(IAddressSelector)).SingleInstance();
            return builder;
        }

        /// <summary>
        /// 使用压力最小优先分配轮询的地址选择器。
        /// </summary>
        /// <param name="builder">服务构建者。</param>
        /// <returns>服务构建者。</returns>
        public static IServiceBuilder UseFairPollingAddressSelector(this IServiceBuilder builder)
        {
            builder.Services.RegisterType(typeof(FairPollingAdrSelector))
                .Named(AddressSelectorMode.FairPolling.ToString(), typeof(IAddressSelector)).SingleInstance();
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
            return builder.UseRandomAddressSelector().UsePollingAddressSelector().UseFairPollingAddressSelector().UseHashAlgorithmAddressSelector();
        }

        #endregion AddressSelector

        #region Configuration Watch

        public static IServiceBuilder AddConfigurationWatch(this IServiceBuilder builder)
        {
            var services = builder.Services;
            services.RegisterType(typeof(ConfigurationWatchManager)).As(typeof(IConfigurationWatchManager)).SingleInstance();
            return builder;
        }
        #endregion

        #region Codec Factory

        /// <summary>
        /// 使用编解码器。
        /// </summary>
        /// <param name="builder">服务构建者。</param>
        /// <param name="codecFactory"></param>
        /// <returns>服务构建者。</returns>
        public static IServiceBuilder UseCodec(this IServiceBuilder builder, ITransportMessageCodecFactory codecFactory)
        {
            builder.Services.RegisterInstance(codecFactory).SingleInstance();
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
            builder.Services.RegisterAdapter(codecFactory).SingleInstance();
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
            services.RegisterType(typeof(DefaultHealthCheckService)).As(typeof(IHealthCheckService)).SingleInstance();
            services.RegisterType(typeof(DefaultAddressResolver)).As(typeof(IAddressResolver)).SingleInstance();
            services.RegisterType(typeof(RemoteInvokeService)).As(typeof(IRemoteInvokeService)).SingleInstance();
            return builder.UseAddressSelector().AddRuntime().AddClusterSupport();
        }

        /// <summary>
        /// 添加集群支持
        /// </summary>
        /// <param name="builder">服务构建者</param>
        /// <returns>服务构建者。</returns>
        public static IServiceBuilder AddClusterSupport(this IServiceBuilder builder)
        {
            var services = builder.Services;
            services.RegisterType(typeof(ServiceCommandProvider)).As(typeof(IServiceCommandProvider)).SingleInstance();
            services.RegisterType(typeof(BreakeRemoteInvokeService)).As(typeof(IBreakeRemoteInvokeService)).SingleInstance();
            services.RegisterType(typeof(FailoverInjectionInvoker)).AsImplementedInterfaces()
                .Named(StrategyType.Injection.ToString(), typeof(IClusterInvoker)).SingleInstance();
            services.RegisterType(typeof(FailoverHandoverInvoker)).AsImplementedInterfaces()
            .Named(StrategyType.Failover.ToString(), typeof(IClusterInvoker)).SingleInstance();
            return builder;
        }

        public static IServiceBuilder AddFilter(this IServiceBuilder builder, IFilter filter)
        {
            var services = builder.Services;
            services.Register(p => filter).As(typeof(IFilter)).SingleInstance();
            if (typeof(IExceptionFilter).IsAssignableFrom(filter.GetType()))
            {
                var exceptionFilter = filter as IExceptionFilter;
                services.Register(p => exceptionFilter).As(typeof(IExceptionFilter)).SingleInstance();
            }
            else if (typeof(IAuthorizationFilter).IsAssignableFrom(filter.GetType()))
            {
                var exceptionFilter = filter as IAuthorizationFilter;
                services.Register(p => exceptionFilter).As(typeof(IAuthorizationFilter)).SingleInstance();
            }
            return builder;
        }

        public static IServiceBuilder AddServiceEngine(this IServiceBuilder builder, Type engine)
        {
            var services = builder.Services;
            services.RegisterType(engine).As(typeof(IServiceEngine)).SingleInstance();
            builder.Services.RegisterType(typeof(DefaultServiceEngineBuilder)).As(typeof(IServiceEngineBuilder)).SingleInstance();
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
            builder.Services.RegisterType(typeof(DefaultServiceExecutor)).As(typeof(IServiceExecutor))
                .Named<IServiceExecutor>(CommunicationProtocol.Tcp.ToString()).SingleInstance();

            return builder.RegisterServices().RegisterRepositories().RegisterServiceBus().RegisterModules().RegisterInstanceByConstraint().AddRuntime();
        }

        /// <summary>
        /// 添加关联服务运行时 
        /// </summary>
        /// <param name="builder">服务构建者。</param>
        /// <returns>服务构建者。</returns>
        public static IServiceBuilder AddRelateServiceRuntime(this IServiceBuilder builder)
        {
            var services = builder.Services;
            services.RegisterType(typeof(DefaultHealthCheckService)).As(typeof(IHealthCheckService)).SingleInstance();
            services.RegisterType(typeof(DefaultAddressResolver)).As(typeof(IAddressResolver)).SingleInstance();
            services.RegisterType(typeof(RemoteInvokeService)).As(typeof(IRemoteInvokeService)).SingleInstance();
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
            services.RegisterType(typeof(ServiceTokenGenerator)).As(typeof(IServiceTokenGenerator)).SingleInstance();
            services.RegisterType(typeof(HashAlgorithm)).As(typeof(IHashAlgorithm)).SingleInstance();
            services.RegisterType(typeof(ServiceEngineLifetime)).As(typeof(IServiceEngineLifetime)).SingleInstance();
            services.RegisterType(typeof(DefaultServiceHeartbeatManager)).As(typeof(IServiceHeartbeatManager)).SingleInstance();
            return new ServiceBuilder(services)
                .AddJsonSerialization()
                .UseJsonCodec();

        }

        public static IServiceBuilder RegisterInstanceByConstraint(this IServiceBuilder builder, params string[] virtualPaths)
        {
            var services = builder.Services;
            var referenceAssemblies = GetReferenceAssembly(virtualPaths);

            foreach (var assembly in referenceAssemblies)
            {
                services.RegisterAssemblyTypes(assembly)
                 .Where(t => typeof(ISingletonDependency).GetTypeInfo().IsAssignableFrom(t)).AsImplementedInterfaces().AsSelf().SingleInstance();

                services.RegisterAssemblyTypes(assembly)
                .Where(t => typeof(ITransientDependency).GetTypeInfo().IsAssignableFrom(t)).AsImplementedInterfaces().AsSelf().InstancePerDependency();
            }
            return builder;

        }

        private static IServiceBuilder AddRuntime(this IServiceBuilder builder)
        {
            var services = builder.Services;

            services.RegisterType(typeof(ClrServiceEntryFactory)).As(typeof(IClrServiceEntryFactory)).SingleInstance();

            services.Register(provider =>
            {
                try
                {
                    var assemblys = GetReferenceAssembly();
                    var types = assemblys.SelectMany(i => i.ExportedTypes).ToArray();
                    return new AttributeServiceEntryProvider(types, provider.Resolve<IClrServiceEntryFactory>(),
                         provider.Resolve<ILogger<AttributeServiceEntryProvider>>(), provider.Resolve<CPlatformContainer>());
                }
                finally
                {
                    builder = null;
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
        public static IServiceBuilder RegisterServices(this IServiceBuilder builder, params string[] virtualPaths)
        {
            try
            {
                var services = builder.Services;
                var referenceAssemblies = GetAssemblies(virtualPaths);
                foreach (var assembly in referenceAssemblies)
                {
                    services.RegisterAssemblyTypes(assembly)
                       .Where(t => typeof(IServiceKey).GetTypeInfo().IsAssignableFrom(t) && t.IsInterface)
                       .AsImplementedInterfaces();
                    services.RegisterAssemblyTypes(assembly)
                 .Where(t => typeof(IServiceBehavior).GetTypeInfo().IsAssignableFrom(t) && t.GetTypeInfo().GetCustomAttribute<ModuleNameAttribute>() == null).AsImplementedInterfaces();

                    var types = assembly.GetTypes().Where(t => typeof(IServiceBehavior).GetTypeInfo().IsAssignableFrom(t) && t.GetTypeInfo().GetCustomAttribute<ModuleNameAttribute>() != null);
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
            catch (Exception ex)
            {
                if (ex is System.Reflection.ReflectionTypeLoadException)
                {
                    var typeLoadException = ex as ReflectionTypeLoadException;
                    var loaderExceptions = typeLoadException.LoaderExceptions;
                    throw loaderExceptions[0];
                }
                throw ex;
            }
        }

        public static IServiceBuilder RegisterServiceBus
            (this IServiceBuilder builder, params string[] virtualPaths)
        {
            var services = builder.Services;
            var referenceAssemblies = GetAssemblies(virtualPaths);

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
        public static IServiceBuilder RegisterRepositories(this IServiceBuilder builder, params string[] virtualPaths)
        {
            var services = builder.Services;
            var referenceAssemblies = GetAssemblies(virtualPaths);

            foreach (var assembly in referenceAssemblies)
            {
                services.RegisterAssemblyTypes(assembly)
                    .Where(t => typeof(BaseRepository).GetTypeInfo().IsAssignableFrom(t));
            }
            return builder;
        }

        public static IServiceBuilder RegisterModules(this IServiceBuilder builder, params string[] virtualPaths)
        {
            var services = builder.Services;
            var referenceAssemblies = GetAssemblies(virtualPaths);
            if (builder == null) throw new ArgumentNullException("builder");
            var packages = ConvertDictionary(AppConfig.ServerOptions.Packages);
            foreach (var moduleAssembly in referenceAssemblies)
            {
                GetAbstractModules(moduleAssembly).ForEach(p =>
                {
                    services.RegisterModule(p);
                    if (packages.ContainsKey(p.TypeName))
                    {
                        var useModules = packages[p.TypeName];
                        if (useModules.AsSpan().IndexOf(p.ModuleName) >= 0)
                            p.Enable = true;
                        else
                            p.Enable = false;
                    }
                    _modules.Add(p);
                });
            }
            builder.Services.Register(provider => new ModuleProvider(
               _modules, virtualPaths, provider.Resolve<ILogger<ModuleProvider>>(), provider.Resolve<CPlatformContainer>()
                )).As<IModuleProvider>().SingleInstance();
            return builder;
        }

        public static List<Type> GetInterfaceService(this IServiceBuilder builder, params string[] virtualPaths)
        {
            var types = new List<Type>();
            var referenceAssemblies = GetReferenceAssembly(virtualPaths);
            referenceAssemblies.ForEach(p =>
            {
                types.AddRange(p.GetTypes().Where(t => typeof(IServiceKey).GetTypeInfo().IsAssignableFrom(t) && t.IsInterface));
            });
            return types;
        }

        public static IEnumerable<string> GetDataContractName(this IServiceBuilder builder, params string[] virtualPaths)
        {
            var namespaces = new List<string>();
            var assemblies = builder.GetInterfaceService(virtualPaths)
                .Select(p => p.Assembly)
                .Union(GetSystemModules())
                .Distinct()
                .ToList();

            assemblies.ForEach(assembly =>
            {
                namespaces.AddRange(assembly.GetTypes().Where(t => t.GetCustomAttribute<DataContractAttribute>() != null).Select(n => n.Namespace));
            });
            return namespaces;
        }

        private static IDictionary<string, string> ConvertDictionary(List<ModulePackage> list)
        {
            var result = new Dictionary<string, string>();
            list.ForEach(p =>
            {
                result.Add(p.TypeName, p.Using);
            });
            return result;
        }

        private static List<Assembly> GetReferenceAssembly(params string[] virtualPaths)
        {
            var refAssemblies = new List<Assembly>();
            var rootPath = AppContext.BaseDirectory;
            var existsPath = virtualPaths.Any();
            if (existsPath && !string.IsNullOrEmpty(AppConfig.ServerOptions.RootPath))
                rootPath = AppConfig.ServerOptions.RootPath;
            var result = _referenceAssembly;
            if (!result.Any() || existsPath)
            {
                var paths = virtualPaths.Select(m => Path.Combine(rootPath, m)).ToList();
                if (!existsPath) paths.Add(rootPath);
                paths.ForEach(path =>
                {
                    var assemblyFiles = GetAllAssemblyFiles(path);

                    foreach (var referencedAssemblyFile in assemblyFiles)
                    {
                        var referencedAssembly = Assembly.LoadFrom(referencedAssemblyFile);
                        if (!_referenceAssembly.Contains(referencedAssembly))
                            _referenceAssembly.Add(referencedAssembly);
                        refAssemblies.Add(referencedAssembly);
                    }
                    result = existsPath ? refAssemblies : _referenceAssembly;
                });
            }
            return result;
        }

        private static List<Assembly> GetSystemModules()
        {
            var assemblies = new List<Assembly>();
            var referenceAssemblies = GetReferenceAssembly();
            foreach (var referenceAssembly in referenceAssemblies)
            {
                var abstractModules = GetAbstractModules(referenceAssembly);
                if (abstractModules.Any(p => p.GetType().IsSubclassOf(typeof(SystemModule))))
                {
                    assemblies.Add(referenceAssembly);
                }
            }
            return assemblies;
        }

        private static List<Assembly> GetAssemblies(params string[] virtualPaths)
        {
            var referenceAssemblies = new List<Assembly>();
            if (virtualPaths.Any())
            {
                referenceAssemblies = GetReferenceAssembly(virtualPaths);
            }
            else
            {
                string[] assemblyNames = DependencyContext
                    .Default.GetDefaultAssemblyNames().Select(p => p.Name).ToArray();
                assemblyNames = GetFilterAssemblies(assemblyNames);
                foreach (var name in assemblyNames)
                    referenceAssemblies.Add(Assembly.Load(name));
                _referenceAssembly.AddRange(referenceAssemblies.Except(_referenceAssembly));
            }
            return referenceAssemblies;
        }

        private static List<AbstractModule> GetAbstractModules(Assembly assembly)
        {
            var abstractModules = new List<AbstractModule>();
            Type[] arrayModule =
                assembly.GetTypes().Where(
                    t => t.IsSubclassOf(typeof(AbstractModule))).ToArray();
            foreach (var moduleType in arrayModule)
            {
                var abstractModule = (AbstractModule)Activator.CreateInstance(moduleType);
                abstractModules.Add(abstractModule);
            }
            return abstractModules;
        }

        private static string[] GetFilterAssemblies(string[] assemblyNames)
        {
            var notRelatedFile = AppConfig.ServerOptions.NotRelatedAssemblyFiles;
            var relatedFile = AppConfig.ServerOptions.RelatedAssemblyFiles;
            var pattern = string.Format("^Microsoft.\\w*|^System.\\w*|^DotNetty.\\w*|^runtime.\\w*|^ZooKeeperNetEx\\w*|^StackExchange.Redis\\w*|^Consul\\w*|^Newtonsoft.Json.\\w*|^Autofac.\\w*{0}",
               string.IsNullOrEmpty(notRelatedFile) ? "" : $"|{notRelatedFile}");
            Regex notRelatedRegex = new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Regex relatedRegex = new Regex(relatedFile, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            if (!string.IsNullOrEmpty(relatedFile))
            {
                return
                    assemblyNames.Where(
                        name => !notRelatedRegex.IsMatch(name) && relatedRegex.IsMatch(name)).ToArray();
            }
            else
            {
                return
                    assemblyNames.Where(
                        name => !notRelatedRegex.IsMatch(name)).ToArray();
            }
        }

        private static List<string> GetAllAssemblyFiles(string parentDir)
        {
            var notRelatedFile = AppConfig.ServerOptions.NotRelatedAssemblyFiles;
            var relatedFile = AppConfig.ServerOptions.RelatedAssemblyFiles;
            var pattern = string.Format("^Microsoft.\\w*|^System.\\w*|^Netty.\\w*|^Autofac.\\w*{0}",
               string.IsNullOrEmpty(notRelatedFile) ? "" : $"|{notRelatedFile}");
            Regex notRelatedRegex = new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Regex relatedRegex = new Regex(relatedFile, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            if (!string.IsNullOrEmpty(relatedFile))
            {
                return
                    Directory.GetFiles(parentDir, "*.dll").Select(Path.GetFullPath).Where(
                        a => !notRelatedRegex.IsMatch(a) && relatedRegex.IsMatch(a)).ToList();
            }
            else
            {
                return
                    Directory.GetFiles(parentDir, "*.dll").Select(Path.GetFullPath).Where(
                        a => !notRelatedRegex.IsMatch(Path.GetFileName(a))).ToList();
            }
        }
    }
}