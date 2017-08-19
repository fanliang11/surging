using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Surging.Core.Caching.Configurations;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Runtime.Server;
using System.Net;
using Microsoft.Extensions.Logging;
using Surging.Core.System.Ioc;
using Surging.Core.DotNetty;
using Surging.Core.Zookeeper.Configurations;
using Surging.Core.Zookeeper;
using Surging.Core.ApiGateWay;
using Surging.Core.ProxyGenerator.Utilitys;
using Microsoft.AspNetCore.StaticFiles;

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
              .AddCacheFile("cacheSettings.json", optional: false)
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
              .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            Configuration = builder.Build();
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            return RegisterAutofac(services);
        }

        private IServiceProvider RegisterAutofac(IServiceCollection services)
        {
            services.AddMvc();
            services.AddLogging();
            var builder = new ContainerBuilder();
            builder.Initialize();
            builder.RegisterServices();
            builder.RegisterRepositories();
            builder.RegisterModules();
            builder.Populate(services);
            builder.AddMicroService(option =>
            {
                option.AddServiceRuntime();
                option.UseZooKeeperManager(new ConfigInfo("127.0.0.1:2181"));
                option.UseDotNettyTransport();
                option.AddApiGateWay();
                builder.Register(p => new CPlatformContainer(ServiceLocator.Current));
            });

            ServiceLocator.Current = builder.Build();
            var serviceHost = ServiceLocator.Current.Resolve<IServiceHost>();
            Task.Factory.StartNew(async () =>
            {
                //启动主机
                await serviceHost.StartAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 99));
            }).Wait();

            return new AutofacServiceProvider(ServiceLocator.Current);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            var myProvider = new FileExtensionContentTypeProvider();
            myProvider.Mappings.Add(".tpl", "text/plain");
            app.UseStaticFiles(new StaticFileOptions() { ContentTypeProvider = myProvider });

            app.UseStaticFiles();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}