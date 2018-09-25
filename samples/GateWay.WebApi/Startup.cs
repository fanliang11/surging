using System;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Newtonsoft.Json.Serialization;
using ApiGateWayConfig = Surging.Core.ApiGateWay.AppConfig;
using Surging.Core.CPlatform;
using Surging.Core.ProxyGenerator;
using Surging.Core.Consul;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.DotNetty;
using Surging.Core.Consul.Configurations;
using Surging.Core.ApiGateWay.OAuth.Implementation.Configurations;
using Autofac.Extensions.DependencyInjection;
using Surging.Core.ApiGateWay;
using Surging.Core.Caching.Configurations;
using Surging.Core.Codec.MessagePack;
using Microsoft.AspNetCore.Mvc;

namespace GateWay.WebApi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
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


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            RegisterAutofac(services);
            // Register the Swagger generator, defining one or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "网关 API", Version = "v1" });
                c.DescribeAllParametersInCamelCase();
            });

            // services.AddResponseCaching();

            //分布式缓存   Microsoft.Extensions.Caching.Redis NuGet package
            /*services.AddDistributedRedisCache(options =>
            {
                options.Configuration = "servername";
                options.InstanceName = "Shopping";
            });*/
            //基于  的身份认证
            /*           services.AddCookieAuthentication(CookieAuthenticationDefaults.Authenticatio
           nScheme, options =>
           {
               options.LoginPath = "/Account/Login/";
               options.AccessDeniedPath = "/Account/Forbidden/";
               options.LogoutPath = "/Account/Logout";
               options.ReturnUrlParameter = "ReturnUrl";
           });

            Microsoft.AspNetCore.Identity.EntityFrameworkCore

            services.AddEntityFramework()
.AddSqlServer()
.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(Configuration["Data:DefaultConnection:ConnectionString
"]));
services.AddIdentity<ApplicationUser, IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();


            services.AddCors();







            */
        }

        private IServiceProvider RegisterAutofac(IServiceCollection services)
        {
            var registerConfig = ApiGateWayConfig.Register;
            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(CustomExceptionFilterAttribute));
                //options.CacheProfiles.Add("5minutes", new CacheProfile
                //{
                //    Duration = 5 * 60,
                //    Location = ResponseCacheLocation.Any,
                //    VaryByHeader = "Accept-Language"
                //});
                //[ResponseCache(CacheProfileName = "5minutes")]
            }).AddJsonOptions(options =>
            {
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            });
            services.AddLogging();
            var builder = new ContainerBuilder();
            builder.Populate(services);
            builder.AddMicroService(option =>
            {
                option.AddClient();
               // option.AddClientIntercepted(typeof(CacheProviderInterceptor));
                option.UseConsulManager(new ConfigInfo(registerConfig.Address));
                option.UseDotNettyTransport();
                option.AddApiGateWay();
                option.UseJsonCodec();
               // option.UseMessagePackCodec();
                builder.Register(m => new CPlatformContainer(ServiceLocator.Current));
            });
            ServiceLocator.Current = builder.Build();
            return new AutofacServiceProvider(ServiceLocator.Current);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();


            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            // app.UseAuthentication();
            //app.UseCors(builder => builder.WithOrigins("http://mysite.com"));
            //app.UseResponseCaching();
            app.UseMvc();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "网关  API V1");
            });

        }
    }
}
