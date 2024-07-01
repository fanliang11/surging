using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Surging.ApiGateway;
using Surging.Apm.Skywalking;
using Surging.Apm.Skywalking.Abstractions;
using Surging.Core.ApiGateWay;
using Surging.Core.ApiGateWay.Configurations;
using Surging.Core.ApiGateWay.OAuth.Implementation.Configurations;
using Surging.Core.Caching;
using Surging.Core.Caching.Configurations;
using Surging.Core.Codec.MessagePack;
using Surging.Core.Consul;
using Surging.Core.Consul.Configurations;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Cache;
using Surging.Core.CPlatform.Serialization.JsonConverters;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.DotNetty;
using Surging.Core.ProxyGenerator;
using Surging.Core.Zookeeper;
using ApiGateWayConfig = Surging.Core.ApiGateWay.AppConfig;
using ZookeeperConfigInfo = Surging.Core.Zookeeper.Configurations.ConfigInfo;

namespace Surging.ApiGateway
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public IContainer ApplicationContainer { get; private set; }

        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
              .SetBasePath(env.ContentRootPath)
              .AddCacheFile("Configs/cacheSettings.json", optional: false)
              .AddJsonFile("Configs/appsettings.json", optional: true, reloadOnChange: true)
              .AddGatewayFile("Configs/gatewaySettings.json", optional: false)
              .AddJsonFile($"Configs/appsettings.{env.EnvironmentName}.json", optional: true);
            Configuration = builder.Build();
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            return RegisterAutofac(services);
        }

        private IServiceProvider RegisterAutofac(IServiceCollection services)
        {
            var registerConfig = ApiGateWayConfig.Register;
            services.AddMvc(options => {
                options.Filters.Add(typeof(CustomExceptionFilterAttribute));
                options.EnableEndpointRouting = false;
            }).AddJsonOptions(options => {
                options.JsonSerializerOptions.Converters.Add(new DateTimeJsonConverter("yyyy-MM-dd HH:mm:ss"));
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
                options.JsonSerializerOptions.DictionaryKeyPolicy = null;
            });
            services.AddLogging(opt =>
            {
                opt.AddConsole();
            });
            services.AddCors();
            var builder = new ContainerBuilder();
            builder.Populate(services);
            builder.AddMicroService(option =>
            {
                option.AddClient();
                option.AddCache();
                //option.UseZooKeeperManager(new ConfigInfo("127.0.0.1:2181"));
                if (registerConfig.Provider == RegisterProvider.Consul)
                    option.UseConsulManager(new ConfigInfo(registerConfig.Address, enableChildrenMonitor: false));
                else if (registerConfig.Provider == RegisterProvider.Zookeeper)
                    option.UseZooKeeperManager(new ZookeeperConfigInfo(registerConfig.Address, enableChildrenMonitor: true));
                option.UseDotNettyTransport();
                option.AddApiGateWay();
                option.AddRpcTransportDiagnostic();
                option.UseSkywalking();
                option.AddFilter(new ServiceExceptionFilter());
                //option.UseProtoBufferCodec();
                option.UseMessagePackCodec();
                builder.Register(m => new CPlatformContainer(ServiceLocator.Current));
            });
            ServiceLocator.Current = builder.Build();
            return new AutofacServiceProvider(ServiceLocator.Current);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            var serviceCacheProvider = ServiceLocator.Current.Resolve<ICacheNodeProvider>();
            var addressDescriptors = serviceCacheProvider.GetServiceCaches().ToList();
            ServiceLocator.Current.Resolve<IServiceProxyFactory>();
            ServiceLocator.Current.Resolve<IServiceCacheManager>().SetCachesAsync(addressDescriptors);
            ServiceLocator.Current.Resolve<IConfigurationWatchProvider>();
            ServiceLocator.Current.Resolve<IInstrumentStartup>().StartAsync();
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
                var policy = Surging.Core.ApiGateWay.AppConfig.Policy;
                if (policy.Origins != null)
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
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
