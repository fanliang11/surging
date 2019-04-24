using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Surging.Core.ApiGateWay;
using Surging.Core.ApiGateWay.Configurations;
using Surging.Core.ApiGateWay.OAuth.Implementation.Configurations;
using Surging.Core.Caching.Configurations;
using Surging.Core.Codec.MessagePack;
using Surging.Core.Consul;
using Surging.Core.Consul.Configurations;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.DotNetty;
using Surging.Core.ProxyGenerator;
using Surging.Core.System.Intercept;
using Surging.Core.Zookeeper;
//using Surging.Core.Zookeeper;
using ZookeeperConfigInfo =  Surging.Core.Zookeeper.Configurations.ConfigInfo;
using System;
using ApiGateWayConfig = Surging.Core.ApiGateWay.AppConfig;
using Surging.Core.Caching;
using Surging.Core.CPlatform.Cache;
using System.Linq;

namespace Surging.ApiGateway
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public IContainer ApplicationContainer { get; private set; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
              .SetBasePath(env.ContentRootPath)
              .AddCacheFile("Configs/cacheSettings.json", optional: false)
              .AddJsonFile("Configs/appsettings.json", optional: true, reloadOnChange: true)
              .AddGatewayFile("Configs/gatewaySettings.json", optional: false)
              .AddJsonFile($"Configs/appsettings.{env.EnvironmentName}.json", optional: true);
            Configuration = builder.Build();
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            return RegisterAutofac(services);
        }

        private IServiceProvider RegisterAutofac(IServiceCollection services)
        {
            var registerConfig = ApiGateWayConfig.Register;
            services.AddMvc(options => {
                options.Filters.Add(typeof(CustomExceptionFilterAttribute));
            }).AddJsonOptions(options => {
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            });
            services.AddLogging();
            services.AddCors();
            var builder = new ContainerBuilder();
            builder.Populate(services); 
            builder.AddMicroService(option =>
            {
                option.AddClient();
                option.AddCache();
                option.AddClientIntercepted(typeof(CacheProviderInterceptor));
                //option.UseZooKeeperManager(new ConfigInfo("127.0.0.1:2181"));
               if(registerConfig.Provider== RegisterProvider.Consul)
                option.UseConsulManager(new ConfigInfo(registerConfig.Address,enableChildrenMonitor:false));
               else if(registerConfig.Provider == RegisterProvider.Zookeeper)
                    option.UseZooKeeperManager(new ZookeeperConfigInfo(registerConfig.Address, enableChildrenMonitor: true));
                option.UseDotNettyTransport();
                option.AddApiGateWay();
                option.AddFilter(new ServiceExceptionFilter());
                //option.UseProtoBufferCodec();
                option.UseMessagePackCodec();
                builder.Register(m => new CPlatformContainer(ServiceLocator.Current));
            });
            ServiceLocator.Current = builder.Build();
            return new AutofacServiceProvider(ServiceLocator.Current);

        }
        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            var serviceCacheProvider = ServiceLocator.Current.Resolve<ICacheNodeProvider>();
            var addressDescriptors = serviceCacheProvider.GetServiceCaches().ToList();
            ServiceLocator.Current.Resolve<IServiceCacheManager>().SetCachesAsync(addressDescriptors);
            ServiceLocator.Current.Resolve<IConfigurationWatchProvider>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseCors(builder =>
            {
                var policy = Core.ApiGateWay.AppConfig.Policy;
                builder.WithOrigins(policy.Origins);
                if (policy.AllowAnyHeader)
                    builder.AllowAnyHeader();
                if (policy.AllowAnyMethod)
                    builder.AllowAnyMethod();
                if (policy.AllowAnyOrigin)
                    builder.AllowAnyOrigin();
                if (policy.AllowCredentials)
                    builder.AllowCredentials();
            });
            var myProvider = new FileExtensionContentTypeProvider();
            myProvider.Mappings.Add(".tpl", "text/plain");
            app.UseStaticFiles(new StaticFileOptions() { ContentTypeProvider = myProvider });
            app.UseStaticFiles();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(
                "Path",
                "{*path}",
                new { controller = "Services", action = "Path" });
            });
        }
    }
}