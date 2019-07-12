using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Module;
using Surging.Core.ProxyGenerator;
using Surging.Core.System.Intercept;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.IModuleServices.Common
{
    /// <summary>
    /// Defines the <see cref="IntercepteModule" />
    /// </summary>
    public class IntercepteModule : SystemModule
    {
        #region 方法

        /// <summary>
        /// The Initialize
        /// </summary>
        /// <param name="context">The context<see cref="AppModuleContext"/></param>
        public override void Initialize(AppModuleContext context)
        {
            base.Initialize(context);
        }

        /// <summary>
        /// Inject dependent third-party components
        /// </summary>
        /// <param name="builder"></param>
        protected override void RegisterBuilder(ContainerBuilderWrapper builder)
        {
            base.RegisterBuilder(builder);
            //builder.AddClientIntercepted(typeof(CacheProviderInterceptor));
            builder.AddClientIntercepted(typeof(LogProviderInterceptor));
        }

        #endregion 方法
    }
}