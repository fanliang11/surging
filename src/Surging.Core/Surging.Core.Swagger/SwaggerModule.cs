using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Module;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.KestrelHttpServer;
using Surging.Core.Swagger.Builder;
using Surging.Core.Swagger.Internal;
using Surging.Core.Swagger.SwaggerUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Surging.Core.Swagger
{
    /// <summary>
    /// Defines the <see cref="SwaggerModule" />
    /// </summary>
    public class SwaggerModule : KestrelHttpModule
    {
        #region 字段

        /// <summary>
        /// Defines the _serviceEntryProvider
        /// </summary>
        private IServiceEntryProvider _serviceEntryProvider;

        /// <summary>
        /// Defines the _serviceSchemaProvider
        /// </summary>
        private IServiceSchemaProvider _serviceSchemaProvider;

        #endregion 字段

        #region 方法

        /// <summary>
        /// The Initialize
        /// </summary>
        /// <param name="context">The context<see cref="ApplicationInitializationContext"/></param>
        public override void Initialize(ApplicationInitializationContext context)
        {
            var info = AppConfig.SwaggerConfig.Info == null
          ? AppConfig.SwaggerOptions : AppConfig.SwaggerConfig.Info;
            if (info != null)
            {
                context.Builder.UseSwagger();
                context.Builder.UseSwaggerUI(c =>
                {
                    var areaName = AppConfig.SwaggerConfig.Options?.IngressName;
                    c.SwaggerEndpoint($"../swagger/{info.Version}/swagger.json", info.Title, areaName);
                    c.SwaggerEndpoint(_serviceEntryProvider.GetALLEntries(), areaName);
                });
            }
        }

        /// <summary>
        /// The Initialize
        /// </summary>
        /// <param name="context">The context<see cref="AppModuleContext"/></param>
        public override void Initialize(AppModuleContext context)
        {
            var serviceProvider = context.ServiceProvoider;
            _serviceSchemaProvider = serviceProvider.GetInstances<IServiceSchemaProvider>();
            _serviceEntryProvider = serviceProvider.GetInstances<IServiceEntryProvider>();
        }

        /// <summary>
        /// The RegisterBuilder
        /// </summary>
        /// <param name="context">The context<see cref="ConfigurationContext"/></param>
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
                    options.SwaggerDoc(info.Version, info);
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

        #endregion 方法
    }
}