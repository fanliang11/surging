using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Engines;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Serialization;
using Surging.Core.KestrelHttpServer.Internal;
using Surging.Core.Swagger.Builder;
using Surging.Core.Swagger.SwaggerUI;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Surging.Core.KestrelHttpServer
{
    public class KestrelHttpMessageListener : HttpMessageListener, IDisposable
    {
        private readonly ILogger<KestrelHttpMessageListener> _logger;
        private IWebHost _host;
        private readonly ISerializer<string> _serializer;
        private readonly IServiceSchemaProvider _serviceSchemaProvider;
        private readonly IServiceEngineLifetime _lifetime;
        private readonly IServiceEntryProvider _serviceEntryProvider;

        public KestrelHttpMessageListener(ILogger<KestrelHttpMessageListener> logger, 
            ISerializer<string> serializer,
            IServiceSchemaProvider serviceSchemaProvider, IServiceEngineLifetime lifetime, IServiceEntryProvider serviceEntryProvider) :base(logger, serializer)
        {
            _logger = logger;
            _serializer = serializer;
            _serviceSchemaProvider = serviceSchemaProvider;
            _lifetime = lifetime;
            _serviceEntryProvider = serviceEntryProvider;
        }
        
        public async Task StartAsync(EndPoint endPoint)
        {
            var ipEndPoint = endPoint as IPEndPoint; 
            try
            {
               var hostBuilder = new WebHostBuilder()
                 .UseContentRoot(Directory.GetCurrentDirectory())
                 .UseKestrel(options => {
                     options.Listen(ipEndPoint);

                 })
                 .ConfigureServices(ConfigureServices)
                 .ConfigureLogging((logger) => {
                     logger.AddConfiguration(
                            CPlatform.AppConfig.GetSection("Logging"));
                 })
                 .Configure(AppResolve);

                if (Directory.Exists(CPlatform.AppConfig.ServerOptions.WebRootPath))
                    hostBuilder = hostBuilder.UseWebRoot(CPlatform.AppConfig.ServerOptions.WebRootPath); 
                _host= hostBuilder.Build();
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
             services.AddMvc();
            if (AppConfig.SwaggerOptions != null)
            {
                services.AddSwaggerGen(options =>
                {
                    options.SwaggerDoc(AppConfig.SwaggerOptions.Version, AppConfig.SwaggerOptions);
                    options.GenerateSwaggerDoc(_serviceEntryProvider.GetALLEntries());
                    options.DocInclusionPredicateV2((docName, apiDesc) =>
                    {
                        if (docName == AppConfig.SwaggerOptions.Version)
                            return true;
                        var assembly = apiDesc.Type.Assembly;

                        var title = assembly
                            .GetCustomAttributes(true)
                            .OfType<AssemblyTitleAttribute>();

                        return title.Any(v => v.Title== docName);
                    });
                    var xmlPaths = _serviceSchemaProvider.GetSchemaFilesPath();
                    foreach (var xmlPath in xmlPaths)
                        options.IncludeXmlComments(xmlPath);
                });
            }
        }

        private void AppResolve(IApplicationBuilder app)
        {
            app.UseStaticFiles();
            app.UseMvc();
            if (AppConfig.SwaggerOptions != null)
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint($"/swagger/{AppConfig.SwaggerOptions.Version}/swagger.json", AppConfig.SwaggerOptions.Title);
                    c.SwaggerEndpoint(_serviceEntryProvider.GetALLEntries()); 
                });
            }
       
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
