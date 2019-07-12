using Microsoft.AspNetCore.Builder;
using System;

namespace Surging.Core.Swagger.Builder
{
    /// <summary>
    /// Defines the <see cref="SwaggerBuilderExtensions" />
    /// </summary>
    public static class SwaggerBuilderExtensions
    {
        #region 方法

        /// <summary>
        /// The UseSwagger
        /// </summary>
        /// <param name="app">The app<see cref="IApplicationBuilder"/></param>
        /// <param name="setupAction">The setupAction<see cref="Action{SwaggerOptions}"/></param>
        /// <returns>The <see cref="IApplicationBuilder"/></returns>
        public static IApplicationBuilder UseSwagger(
            this IApplicationBuilder app,
            Action<SwaggerOptions> setupAction = null)
        {
            if (setupAction == null)
            {
                // Don't pass options so it can be configured/injected via DI container instead
                app.UseMiddleware<SwaggerMiddleware>();
            }
            else
            {
                // Configure an options instance here and pass directly to the middleware
                var options = new SwaggerOptions();
                setupAction.Invoke(options);

                app.UseMiddleware<SwaggerMiddleware>(options);
            }

            return app;
        }

        #endregion 方法
    }
}