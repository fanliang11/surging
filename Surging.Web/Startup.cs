using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Surging.Core.Caching.Configurations;
using Surging.Core.Caching.NetCache;
using Surging.Core.Caching;
using Autofac;
using Surging.Core.System.Ioc;
using Autofac.Extensions.DependencyInjection;
using Surging.Core.CPlatform;
using Surging.Core.DotNetty;
using System.Net;
using Surging.Core.CPlatform.Runtime.Server;

namespace Surging.Web
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
            services.AddLogging();
            var builder = new ContainerBuilder();
            builder.Initialize();
            builder.RegisterServices();
            builder.RegisterRepositories();
            builder.RegisterModules();
            builder.Populate(services);
           var serviceBulider= builder.AddCoreServce()
                .AddServiceRuntime()
                .UseSharedFileRouteManager("127.0.0.1","22")
                .UseDotNettyTransport();
            builder.Register(p => new CPlatformContainer(this.ApplicationContainer));
            
            this.ApplicationContainer = builder.Build();
            var serviceHost=  this.ApplicationContainer.Resolve<IServiceHost>();
            Task.Factory.StartNew(async () =>
            {
                //启动主机
                await serviceHost.StartAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 98));
            }).Wait();
            return new AutofacServiceProvider(this.ApplicationContainer);
        }

            public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}
