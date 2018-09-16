using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Surging.Core.CPlatform.Runtime.Server;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.KestrelHttpServer.Builder
{
   public static class MvcApplicationBuilderExtensions
    {
        private const string EndpointRoutingRegisteredKey = "__EndpointRoutingMiddlewareRegistered";

        /// <summary>
        /// Adds MVC to the <see cref="IApplicationBuilder"/> request execution pipeline.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/>.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <remarks>This method only supports attribute routing. To add conventional routes use
        /// <see cref="UseMvc(IApplicationBuilder, Action{IRouteBuilder})"/>.</remarks>
        public static IApplicationBuilder UseWebService(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            //app.ApplicationServices.GetRequiredService<>
            return app.UseWebService(routes =>
            {
                //routes.get
            });
        }
         

        /// <summary>
        /// Adds MVC to the <see cref="IApplicationBuilder"/> request execution pipeline.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/>.</param>
        /// <param name="configureRoutes">A callback to configure MVC routes.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IApplicationBuilder UseWebService(
            this IApplicationBuilder app,
            Action<IServiceEntryProvider> configureRoutes)
        {
            return app;
        }
    }
}
