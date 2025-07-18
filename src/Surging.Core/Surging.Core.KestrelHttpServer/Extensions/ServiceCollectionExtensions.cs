﻿using Microsoft.Extensions.DependencyInjection;
using Surging.Core.KestrelHttpServer.Filters;
using Surging.Core.KestrelHttpServer.Interceptors;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.KestrelHttpServer.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddFilters(this IServiceCollection serviceCollection, Type filter)
        {
            if (typeof(IAuthorizationFilter).IsAssignableFrom(filter))
            {
                serviceCollection.AddSingleton(typeof(IAuthorizationFilter), filter);
            }
            else if (typeof(IActionFilter).IsAssignableFrom(filter))
            {
                serviceCollection.AddSingleton(typeof(IActionFilter), filter);
            }
            else if (typeof(IExceptionFilter).IsAssignableFrom(filter))
            {
                serviceCollection.AddSingleton(typeof(IExceptionFilter), filter);
            }
        }

        public static void AddHttpInterceptors(this IServiceCollection serviceCollection, Type interceptor)
        {
            if (typeof(IHttpInterceptor).IsAssignableFrom(interceptor))
            {
                serviceCollection.AddSingleton(typeof(IHttpInterceptor), interceptor);
            }
        }
    }
}
