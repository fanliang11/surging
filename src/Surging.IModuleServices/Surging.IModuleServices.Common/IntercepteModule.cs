using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Module;
using Surging.Core.ProxyGenerator;
using Surging.Core.System.Intercept;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.IModuleServices.Common
{
    [ModuleMetadata(Description ="",Title ="缓存权限配置")]
    public class IntercepteModule : SystemModule
    {
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
            builder.AddClientIntercepted(typeof(CacheProviderInterceptor));
            builder.AddFilter(typeof(PermissionAttribute));
           //builder.AddClientIntercepted(typeof(LogProviderInterceptor));
        }
    }
}

