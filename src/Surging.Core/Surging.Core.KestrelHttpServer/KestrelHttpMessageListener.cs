using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Transport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Surging.Core.KestrelHttpServer.Builder;
using Microsoft.AspNetCore.Http;
using Surging.Core.CPlatform.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Surging.Core.Swagger;
using Surging.Core.Swagger.Builder;
using Surging.Core.Swagger.SwaggerUI;
using Autofac;

namespace Surging.Core.KestrelHttpServer
{
    public class KestrelHttpMessageListener : HttpMessageListener, IDisposable
    {
        private readonly ILogger<KestrelHttpMessageListener> _logger;
        private IWebHost _host;
        private readonly ISerializer<string> _serializer;


        public KestrelHttpMessageListener(ILogger<KestrelHttpMessageListener> logger, 
            ISerializer<string> serializer) :base(logger, serializer)
        {
            _logger = logger;
            _serializer = serializer;
        }
        
        public async Task StartAsync(EndPoint endPoint)
        {
            var ipEndPoint = endPoint as IPEndPoint; 
            try
            {
                _host = new WebHostBuilder()
                 .UseContentRoot(Directory.GetCurrentDirectory())
                 .UseKestrel(options=> {
                     options.Listen(ipEndPoint);

                 })
                 .ConfigureServices(ConfigureServices)
                 .ConfigureLogging((logger) => {
                     logger.AddConfiguration(
                            CPlatform.AppConfig.GetSection("Logging"));
                 })
                 .Configure(AppResolve)
                 .Build();

               await _host.RunAsync();
            }
            catch
            {
                _logger.LogError($"http服务主机启动失败，监听地址：{endPoint}。 ");
            }

        }

        public void ConfigureServices(IServiceCollection services)
        {
             services.AddMvc();
            services.AddSwaggerGen(options =>
            {

                options.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = " API 文档",
                    Description = "by bj eland"
                });
                var basePath = AppContext.BaseDirectory;
                var xmlPath = Path.Combine(basePath, "Surging.IModuleServices.Common.xml"); 
                options.IncludeXmlComments(xmlPath); 

            });
            //  services.ConfigureSwaggerGen(options =>
            //{
            //    options.SwaggerDoc("v1", new Info
            //    {
            //        Version = "v1",
            //        Title = "测试ASP.NET Core WebAPI生成文档（文档说明）",
            //        Description = "A simple example ASP.NET Core Web API",
            //        TermsOfService = "None",
            //        Contact = new Contact { Name = "linyongjie", Email = "", Url = "https://docs.microsoft.com/zh-cn/aspnet/core/" },
            //        License = new License { Name = "Swagger官网", Url = "http://swagger.io/" }
            //    });

            //    //var basePath = Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationBasePath; // 获取到应用程序的根路径
            //    //options.IncludeXmlComments(basePath + "\\dotNetCore_Test1.xml");  //是需要设置 XML 注释文件的完整路径
            //});
        }

        private void AppResolve(IApplicationBuilder app)
        {
            app.UseStaticFiles();
            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "DemoApi");
            });
       
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
