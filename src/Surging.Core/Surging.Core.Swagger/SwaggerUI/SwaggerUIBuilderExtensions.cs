using Microsoft.AspNetCore.Builder;
using System;

namespace Surging.Core.Swagger.SwaggerUI
{
    /// <summary>
    /// Defines the <see cref="SwaggerUIBuilderExtensions" />
    /// </summary>
    public static class SwaggerUIBuilderExtensions
    {
        #region 方法

        /// <summary>
        /// The UseSwaggerUI
        /// </summary>
        /// <param name="app">The app<see cref="IApplicationBuilder"/></param>
        /// <param name="setupAction">The setupAction<see cref="Action{SwaggerUIOptions}"/></param>
        /// <returns>The <see cref="IApplicationBuilder"/></returns>
        public static IApplicationBuilder UseSwaggerUI(
           this IApplicationBuilder app,
           Action<SwaggerUIOptions> setupAction = null)
        {
            if (setupAction == null)
            {
                // Don't pass options so it can be configured/injected via DI container instead
                app.UseMiddleware<SwaggerUIMiddleware>();
            }
            else
            {
                // Configure an options instance here and pass directly to the middleware
                var options = new SwaggerUIOptions();
                setupAction.Invoke(options);

                app.UseMiddleware<SwaggerUIMiddleware>(options);
            }

            return app;
        }

        #endregion 方法
    }
}