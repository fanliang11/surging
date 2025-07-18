using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Module;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.KestrelHttpServer;
using Surging.Core.Swagger_V5.Builder;
using Surging.Core.Swagger_V5.Internal; 
using Surging.Core.Swagger_V5.SwaggerUI; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Surging.Core.Swagger_V5.DependencyInjection;
using Surging.Core.Swagger_V5.SwaggerGen.Filters;
using Microsoft.OpenApi.Models;
using Surging.Core.Swagger_V5.Swagger.Model;

namespace Surging.Core.Swagger_V5
{
    public class SwaggerModule: KestrelHttpModule
    {
        private  IServiceSchemaProvider _serviceSchemaProvider; 
        private  IServiceEntryProvider _serviceEntryProvider;

        public override void Initialize(AppModuleContext context)
        {
            var serviceProvider = context.ServiceProvoider;
            _serviceSchemaProvider = serviceProvider.GetInstances<IServiceSchemaProvider>();
            _serviceEntryProvider = serviceProvider.GetInstances<IServiceEntryProvider>();
        }

        public override void Initialize(ApplicationInitializationContext context)
        {
            var info = AppConfig.SwaggerConfig.Info == null
          ? AppConfig.SwaggerOptions : AppConfig.SwaggerConfig.Info;
            if (info != null)
            {
                context.Builder.UseSwagger();
                context.Builder.UseSwaggerUI(c =>
                {
                    c.ShowExtensions();
                    var areaName = AppConfig.SwaggerConfig.Options?.IngressName;
                    c.SwaggerEndpoint($"../swagger/{info.Version}/swagger.json", info.Title, areaName);
                    c.SwaggerEndpoint(_serviceEntryProvider.GetALLEntries(), areaName);
                });
            }
        }

        public override void RegisterBuilder(ConfigurationContext context)
        {
            var serviceCollection = context.Services;
            var info = AppConfig.SwaggerConfig.Info == null
                     ? AppConfig.SwaggerOptions : AppConfig.SwaggerConfig.Info;
            var swaggerOptions = AppConfig.SwaggerConfig.Options;
            if (info != null)
            {
                serviceCollection.AddSwaggerGen(options =>
                {
                    // options.OperationFilter<AddAuthorizationOperationFilter>();
                    //添加Authorization
                    var securityScheme = new OpenApiSecurityScheme()
                    {
                        Description = "JWT Authorization header using the ApiKey scheme. Example: \"Authorization: {token}\"",
                        Name = "Authorization", 
                        In = ParameterLocation.Header, 
                        Type = SecuritySchemeType.ApiKey, 
                        Scheme = "ApiKey",
                        BearerFormat = "JWT"
                    };

                    //把所有方法配置为增加bearer头部信息
                    var securityRequirement = new OpenApiSecurityRequirement
                    {
                        {
                                new OpenApiSecurityScheme
                                {
                                    Reference = new OpenApiReference
                                    {
                                        Type = ReferenceType.SecurityScheme,
                                        Id = "Auth"
                                    }
                                },
                                new string[] {}
                        }
                    };
                    options.AddSecurityDefinition("Auth", securityScheme);
                    options.AddSecurityRequirement(securityRequirement);
                    options.SwaggerDoc(info.Version, new OpenApiInfo
                        {

                            Title = info.Title,
                            Contact = new OpenApiContact() { Email = info.Contact.Email, Name = info.Contact.Name, Url = new Uri(info.Contact.Url) },
                            Description = info.Description,
                            License = new OpenApiLicense() { Name = info.License.Name, Url =  new Uri(info.License.Url) },
                            TermsOfService = info.TermsOfService==null?null: new Uri(info.TermsOfService),
                            Version = info.Version
                        });
                    
                    if (swaggerOptions != null && swaggerOptions.IgnoreFullyQualified)
                        options.IgnoreFullyQualified();
                    options.GenerateSwaggerDoc(_serviceEntryProvider.GetALLEntries());
                    options.DocInclusionPredicateV2((docName, apiDesc) =>
                    {
                        if (docName == info.Version)
                            return true;
                        var assembly = apiDesc.Type.Assembly;

                        var title = assembly
                            .GetCustomAttributes(true)
                            .OfType<AssemblyTitleAttribute>();

                        return title.Any(v => v.Title == docName);
                    });
                    var xmlPaths = _serviceSchemaProvider.GetSchemaFilesPath();
                    foreach (var xmlPath in xmlPaths)
                        options.IncludeXmlComments(xmlPath);
                });
            }
        }

        /// <summary>
        /// Inject dependent third-party components
        /// </summary>
        /// <param name="builder"></param>
        protected override void RegisterBuilder(ContainerBuilderWrapper builder)
        {
            var section = CPlatform.AppConfig.GetSection("Swagger");
            if (section.Exists())
            {
                AppConfig.SwaggerOptions = section.Get<Info>();
                AppConfig.SwaggerConfig = section.Get<DocumentConfiguration>();
            }
            builder.RegisterType(typeof(DefaultServiceSchemaProvider)).As(typeof(IServiceSchemaProvider)).SingleInstance();

        }
    }
}
