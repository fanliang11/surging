using Surging.Core.CPlatform.Module;
using Surging.Core.ProxyGenerator.Interceptors;
using Surging.Core.ProxyGenerator.Interceptors.Implementation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.ProxyGenerator
{
    /// <summary>
    /// Defines the <see cref="RegistrationExtensions" />
    /// </summary>
    public static class RegistrationExtensions
    {
        #region 方法

        /// <summary>
        /// The AddClientIntercepted
        /// </summary>
        /// <param name="builder">The builder<see cref="ContainerBuilderWrapper"/></param>
        /// <param name="interceptorServiceType">The interceptorServiceType<see cref="Type"/></param>
        public static void AddClientIntercepted(this ContainerBuilderWrapper builder, Type interceptorServiceType)
        {
            builder.RegisterType(interceptorServiceType).As<IInterceptor>().SingleInstance();
            builder.RegisterType<InterceptorProvider>().As<IInterceptorProvider>().SingleInstance();
        }

        /// <summary>
        /// The AddClientIntercepted
        /// </summary>
        /// <param name="builder">The builder<see cref="ContainerBuilderWrapper"/></param>
        /// <param name="interceptorServiceTypes">The interceptorServiceTypes<see cref="Type[]"/></param>
        public static void AddClientIntercepted(this ContainerBuilderWrapper builder, params Type[] interceptorServiceTypes)
        {
            builder.RegisterTypes(interceptorServiceTypes).As<IInterceptor>().SingleInstance();
            builder.RegisterType<InterceptorProvider>().As<IInterceptorProvider>().SingleInstance();
        }

        #endregion 方法
    }
}