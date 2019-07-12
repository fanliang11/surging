using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Engines;
using Surging.Core.CPlatform.Module;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Serialization;
using Surging.Core.KestrelHttpServer.Extensions;
using Surging.Core.KestrelHttpServer.Internal;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Surging.Core.KestrelHttpServer
{
    /// <summary>
    /// Defines the <see cref="KestrelHttpMessageListener" />
    /// </summary>
    public class KestrelHttpMessageListener : HttpMessageListener, IDisposable
    {
        #region 字段

        /// <summary>
        /// Defines the _container
        /// </summary>
        private readonly CPlatformContainer _container;

        /// <summary>
        /// Defines the _lifetime
        /// </summary>
        private readonly IServiceEngineLifetime _lifetime;

        /// <summary>
        /// Defines the _logger
        /// </summary>
        private readonly ILogger<KestrelHttpMessageListener> _logger;

        /// <summary>
        /// Defines the _moduleProvider
        /// </summary>
        private readonly IModuleProvider _moduleProvider;

        /// <summary>
        /// Defines the _serializer
        /// </summary>
        private readonly ISerializer<string> _serializer;

        /// <summary>
        /// Defines the _serviceRouteProvider
        /// </summary>
        private readonly IServiceRouteProvider _serviceRouteProvider;

        /// <summary>
        /// Defines the _host
        /// </summary>
        private IWebHost _host;

        /// <summary>
        /// Defines the _isCompleted
        /// </summary>
        private bool _isCompleted;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="KestrelHttpMessageListener"/> class.
        /// </summary>
        /// <param name="logger">The logger<see cref="ILogger{KestrelHttpMessageListener}"/></param>
        /// <param name="serializer">The serializer<see cref="ISerializer{string}"/></param>
        /// <param name="lifetime">The lifetime<see cref="IServiceEngineLifetime"/></param>
        /// <param name="moduleProvider">The moduleProvider<see cref="IModuleProvider"/></param>
        /// <param name="serviceRouteProvider">The serviceRouteProvider<see cref="IServiceRouteProvider"/></param>
        /// <param name="container">The container<see cref="CPlatformContainer"/></param>
        public KestrelHttpMessageListener(ILogger<KestrelHttpMessageListener> logger,
            ISerializer<string> serializer,
            IServiceEngineLifetime lifetime,
            IModuleProvider moduleProvider,
            IServiceRouteProvider serviceRouteProvider,
            CPlatformContainer container) : base(logger, serializer, serviceRouteProvider)
        {
            _logger = logger;
            _serializer = serializer;
            _lifetime = lifetime;
            _moduleProvider = moduleProvider;
            _container = container;
            _serviceRouteProvider = serviceRouteProvider;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The ConfigureHost
        /// </summary>
        /// <param name="context">The context<see cref="WebHostBuilderContext"/></param>
        /// <param name="options">The options<see cref="KestrelServerOptions"/></param>
        /// <param name="ipAddress">The ipAddress<see cref="IPAddress"/></param>
        public void ConfigureHost(WebHostBuilderContext context, KestrelServerOptions options, IPAddress ipAddress)
        {
            _moduleProvider.ConfigureHost(new WebHostContext(context, options, ipAddress));
        }

        /// <summary>
        /// The ConfigureServices
        /// </summary>
        /// <param name="services">The services<see cref="IServiceCollection"/></param>
        public void ConfigureServices(IServiceCollection services)
        {
            var builder = new ContainerBuilder();
            services.AddMvc();
            _moduleProvider.ConfigureServices(new ConfigurationContext(services,
                _moduleProvider.Modules,
                _moduleProvider.VirtualPaths,
                AppConfig.Configuration));
            builder.Populate(services);

            builder.Update(_container.Current.ComponentRegistry);
        }

        /// <summary>
        /// The Dispose
        /// </summary>
        public void Dispose()
        {
            _host.Dispose();
        }

        /// <summary>
        /// The StartAsync
        /// </summary>
        /// <param name="address">The address<see cref="IPAddress"/></param>
        /// <param name="port">The port<see cref="int?"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task StartAsync(IPAddress address, int? port)
        {
            try
            {
                var hostBuilder = new WebHostBuilder()
                  .UseContentRoot(Directory.GetCurrentDirectory())
                  .UseKestrel((context, options) =>
                  {
                      if (port != null && port > 0)
                          options.Listen(address, port.Value);
                      ConfigureHost(context, options, address);
                  })
                  .ConfigureServices(ConfigureServices)
                  .ConfigureLogging((logger) =>
                  {
                      logger.AddConfiguration(
                             CPlatform.AppConfig.GetSection("Logging"));
                  })
                  .Configure(AppResolve);

                if (Directory.Exists(CPlatform.AppConfig.ServerOptions.WebRootPath))
                    hostBuilder = hostBuilder.UseWebRoot(CPlatform.AppConfig.ServerOptions.WebRootPath);
                _host = hostBuilder.Build();
                _lifetime.ServiceEngineStarted.Register(async () =>
                {
                    await _host.RunAsync();
                });
            }
            catch
            {
                _logger.LogError($"http服务主机启动失败，监听地址：{address}:{port}。 ");
            }
        }

        /// <summary>
        /// The AppResolve
        /// </summary>
        /// <param name="app">The app<see cref="IApplicationBuilder"/></param>
        private void AppResolve(IApplicationBuilder app)
        {
            app.UseStaticFiles();
            app.UseMvc();
            _moduleProvider.Initialize(new ApplicationInitializationContext(app, _moduleProvider.Modules,
                _moduleProvider.VirtualPaths,
                AppConfig.Configuration));
            app.Run(async (context) =>
            {
                var sender = new HttpServerMessageSender(_serializer, context);
                await OnReceived(sender, context);
            });
        }

        #endregion 方法
    }
}