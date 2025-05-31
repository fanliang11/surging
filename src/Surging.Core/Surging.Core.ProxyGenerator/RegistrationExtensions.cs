using Surging.Core.CPlatform.Module;
using Surging.Core.ProxyGenerator.Interceptors;
using Surging.Core.ProxyGenerator.Interceptors.Implementation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.ProxyGenerator
{
    public static class  RegistrationExtensions
    {
        public static void AddClientIntercepted(this ContainerBuilderWrapper builder,  Type interceptorServiceType)
        { 
            builder.RegisterType(interceptorServiceType).As<IInterceptor>().SingleInstance();
            builder.RegisterType<InterceptorProvider>().As<IInterceptorProvider>().SingleInstance();
        }

        public static void AddClientIntercepted(this ContainerBuilderWrapper builder, params Type[] interceptorServiceTypes)
        { 
            builder.RegisterTypes(interceptorServiceTypes).As<IInterceptor>().SingleInstance();
            builder.RegisterType<InterceptorProvider>().As<IInterceptorProvider>().SingleInstance();
     
        }
    }
}
