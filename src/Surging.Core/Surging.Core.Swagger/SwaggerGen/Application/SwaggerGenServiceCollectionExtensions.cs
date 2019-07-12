using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Surging.Core.Swagger;
using Surging.Core.SwaggerGen;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Defines the <see cref="SwaggerGenServiceCollectionExtensions" />
    /// </summary>
    public static class SwaggerGenServiceCollectionExtensions
    {
        #region 方法

        /// <summary>
        /// The AddSwaggerGen
        /// </summary>
        /// <param name="services">The services<see cref="IServiceCollection"/></param>
        /// <param name="setupAction">The setupAction<see cref="Action{SwaggerGenOptions}"/></param>
        /// <returns>The <see cref="IServiceCollection"/></returns>
        public static IServiceCollection AddSwaggerGen(
            this IServiceCollection services,
            Action<SwaggerGenOptions> setupAction = null)
        {
            // Add Mvc convention to ensure ApiExplorer is enabled for all actions
            services.Configure<MvcOptions>(c =>
                c.Conventions.Add(new SwaggerApplicationConvention()));

            // Register generator and it's dependencies
            services.AddTransient<ISwaggerProvider, SwaggerGenerator>();
            services.AddTransient<ISchemaRegistryFactory, SchemaRegistryFactory>();

            // Register custom configurators that assign values from SwaggerGenOptions (i.e. high level config)
            // to the service-specific options (i.e. lower-level config)
            services.AddTransient<IConfigureOptions<SwaggerGeneratorOptions>, ConfigureSwaggerGeneratorOptions>();
            services.AddTransient<IConfigureOptions<SchemaRegistryOptions>, ConfigureSchemaRegistryOptions>();

            if (setupAction != null) services.ConfigureSwaggerGen(setupAction);

            return services;
        }

        /// <summary>
        /// The ConfigureSwaggerGen
        /// </summary>
        /// <param name="services">The services<see cref="IServiceCollection"/></param>
        /// <param name="setupAction">The setupAction<see cref="Action{SwaggerGenOptions}"/></param>
        public static void ConfigureSwaggerGen(
            this IServiceCollection services,
            Action<SwaggerGenOptions> setupAction)
        {
            services.Configure(setupAction);
        }

        #endregion 方法
    }
}