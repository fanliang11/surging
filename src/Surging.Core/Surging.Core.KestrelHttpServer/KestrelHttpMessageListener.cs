using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Engines;
using Surging.Core.CPlatform.Module;
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
using Surging.Core.CPlatform.Routing;

namespace Surging.Core.KestrelHttpServer
{
    public class KestrelHttpMessageListener : HttpMessageListener, IDisposable
    {
        private readonly ILogger<KestrelHttpMessageListener> _logger;
        private IWebHost _host;
        private bool _isCompleted;
        private readonly ISerializer<string> _serializer;
        private readonly IServiceEngineLifetime _lifetime;
        private readonly IModuleProvider _moduleProvider;
        private readonly CPlatformContainer _container;
        private readonly IServiceRouteProvider _serviceRouteProvider;

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

        public async Task StartAsync(EndPoint endPoint)
        {
            var ipEndPoint = endPoint as IPEndPoint;
            try
            {
                var hostBuilder = new WebHostBuilder()
                  .UseContentRoot(Directory.GetCurrentDirectory())
                  .UseKestrel(options =>
                  {
                      options.Listen(ipEndPoint);

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
                _logger.LogError($"http服务主机启动失败，监听地址：{endPoint}。 ");
            }

        }

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

        public void Dispose()
        {
            _host.Dispose();
        }

    }
}
