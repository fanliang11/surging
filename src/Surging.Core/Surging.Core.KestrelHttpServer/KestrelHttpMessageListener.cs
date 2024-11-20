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
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Surging.Core.KestrelHttpServer.Filters;
using Surging.Core.CPlatform.Messages;
using System.Diagnostics;
using Surging.Core.CPlatform.Configurations;
using Surging.Core.CPlatform.Diagnostics;
using Surging.Core.CPlatform.Utilities;
using Microsoft.AspNetCore.Mvc;
using Surging.Core.CPlatform.Network;
using Microsoft.AspNetCore;
using Autofac.Core;
using Surging.Core.CPlatform.Transport;
using System.Collections.Concurrent;
using Surging.Core.CPlatform.Address;
using Surging.Core.KestrelHttpServer.Runtime;
using System.Net.Http.Headers;
using Surging.Core.KestrelHttpServer.Interceptors;

namespace Surging.Core.KestrelHttpServer
{
    public class KestrelHttpMessageListener : HttpMessageListener, INetwork, IDisposable
    {
        private readonly ILogger<KestrelHttpMessageListener> _logger;
        private IWebHost _host;
        private bool _isCompleted;
        private readonly ISerializer<string> _serializer;
        private readonly IServiceEngineLifetime _lifetime;
        private readonly IModuleProvider _moduleProvider;
        private readonly CPlatformContainer _container;
        private readonly IServiceRouteProvider _serviceRouteProvider;
        private readonly DiagnosticListener _diagnosticListener;
        private readonly IHttpServiceEntryProvider _httpServiceEntryProvider;
        private readonly NetworkProperties _httpServerProperties;
         
        public string Id { get ; set ; }
        public KestrelHttpMessageListener(ILogger<KestrelHttpMessageListener> logger,
            ISerializer<string> serializer,
            IServiceEngineLifetime lifetime,
            IModuleProvider moduleProvider,
            IServiceRouteProvider serviceRouteProvider,
            IHttpServiceEntryProvider httpServiceEntryProvider,
            CPlatformContainer container):this(logger, serializer, lifetime, moduleProvider, serviceRouteProvider, httpServiceEntryProvider, container,new NetworkProperties())
        {

        }
        public KestrelHttpMessageListener(ILogger<KestrelHttpMessageListener> logger,
            ISerializer<string> serializer, 
            IServiceEngineLifetime lifetime,
            IModuleProvider moduleProvider,
            IServiceRouteProvider serviceRouteProvider,
            IHttpServiceEntryProvider httpServiceEntryProvider,
            CPlatformContainer container, NetworkProperties properties) : base(logger, serializer, serviceRouteProvider)
        {
            Id = properties?.Id;
            _logger = logger;
            _serializer = serializer;
            _lifetime = lifetime;
            _moduleProvider = moduleProvider;
            _container = container;
            _httpServiceEntryProvider = httpServiceEntryProvider;
           _serviceRouteProvider = serviceRouteProvider;
            _diagnosticListener = new DiagnosticListener(DiagnosticListenerExtensions.DiagnosticListenerName);
            _httpServerProperties = properties;
        }

        public async Task StartAsync(IPAddress address,int? port)
        { 
            try
            {
                if (AppConfig.ServerOptions.DockerDeployMode == DockerDeployMode.Swarm)
                {
                    address = IPAddress.Any;
                }

                var hostBuilder = new WebHostBuilder()
                  .UseContentRoot(Directory.GetCurrentDirectory())
                  .UseKestrel((context,options) =>
                  {
                      options.Limits.MinRequestBodyDataRate = null;
                      options.Limits.MinResponseDataRate = null;
                      options.Limits.MaxRequestBodySize = null;
                      options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(30);
                      if (port != null && port > 0)
                          options.Listen(address, port.Value, listenOptions =>
                          {
                              listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                          });
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
                    if(port!=null && address !=null)
                    await _host.RunAsync();
                });

            }
            catch
            {
                _logger.LogError($"http服务主机启动失败，监听地址：{address}:{port}。 ");
            }

        }

        public void ConfigureHost(WebHostBuilderContext context, KestrelServerOptions options,IPAddress ipAddress)
        {
            _moduleProvider.ConfigureHost(new WebHostContext(context, options, ipAddress));
        }

        public void ConfigureServices(IServiceCollection services)
        { 
            var builder = new ContainerBuilder();
            services.AddMvc(option=>option.EnableEndpointRouting = false); 
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
            var httpEntry=  _httpServiceEntryProvider.GetEntry();
           _moduleProvider.Initialize(new ApplicationInitializationContext(app, _moduleProvider.Modules,
                _moduleProvider.VirtualPaths,
                AppConfig.Configuration));
            app.Use(async (context, next) =>
            {
                var messageId = Guid.NewGuid().ToString("N");
                var sender = new HttpServerMessageSender(_serializer, context,_diagnosticListener);
                try
                {
                    var httpInterceptors = app.ApplicationServices.GetServices<IHttpInterceptor>();
                    var isIntercept = false;
                    if (!string.IsNullOrEmpty(Id))
                        isIntercept= await OnHttpInterceptor(context, sender,Id, messageId ,httpEntry, httpInterceptors);
                    if (!isIntercept)
                    {
                        var filters = app.ApplicationServices.GetServices<IAuthorizationFilter>();
                        var isSuccess = await OnAuthorization(context, sender, messageId, filters);
                        if (isSuccess)
                        {
                            var actionFilters = app.ApplicationServices.GetServices<IActionFilter>();
                            await OnReceived(sender, messageId, context, actionFilters);
                        }
                        await next();
                    }
                }
                catch (Exception ex)
                {
                    var filters = app.ApplicationServices.GetServices<IExceptionFilter>();
                    WirteDiagnosticError(messageId, ex);
                    await OnException(context, sender, messageId, ex, filters);
                }
           
            });  
            app.Run(context => Task.CompletedTask);
        }

        private void WirteDiagnosticError(string messageId,Exception ex)
        {
            _diagnosticListener.WriteTransportError(CPlatform.Diagnostics.TransportType.Rest, new TransportErrorEventData(new DiagnosticMessage
            {
                Id = messageId
            }, ex));
        }

        public void Dispose()
        {
            _host.Dispose();
        }

        public async Task StartAsync()
        { 
            _moduleProvider.Modules.ForEach(p =>
            {
                if (p.ModuleName== "StageModule")
                    p.Enable = false;
                _httpServerProperties.ParserConfiguration.TryGetValue("enableSwagger", out object enableSwagger);
                _httpServerProperties.ParserConfiguration.TryGetValue("enableWebService", out object enableWebService);
                if (enableSwagger != null && bool.Parse(enableSwagger.ToString()) == false && p.ModuleName == "SwaggerModule")
                    p.Enable = false;
                if (enableWebService != null && bool.Parse(enableWebService.ToString()) == false && p.ModuleName == "WebServiceModule")
                    p.Enable = false;

            });
            var executor = _container.GetInstances<IServiceExecutor>(CommunicationProtocol.Http.ToString());
            var host =new DefaultHttpServiceHost(async endPoint =>
            {
                await StartAsync(_httpServerProperties.CreateSocketAddress().Address, _httpServerProperties.Port);
                return this;
            }, executor, this);
           await host.StartAsync(new IPEndPoint(_httpServerProperties.CreateSocketAddress().Address, _httpServerProperties.Port));
            _moduleProvider.Modules.ForEach(p =>
            {
                if (p.ModuleName == "StageModule" || p.ModuleName == "SwaggerModule" || p.ModuleName == "WebServiceModule")
                    p.Enable = true;
            });
        }

        NetworkType INetwork.GetType()
        {
            return NetworkType.Http;
        }

        public async void Shutdown()
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"Http服务主机已停止。");
            await _host.StopAsync();
        }

        public bool IsAlive()
        {
            return true;
        }

        public bool IsAutoReload()
        {
            return false;
        }
    }
}
