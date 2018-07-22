using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Convertibles;
using Surging.Core.CPlatform.Module;
using Surging.Core.CPlatform.Runtime.Client;
using Surging.Core.ProxyGenerator.Implementation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.ProxyGenerator
{
   public class ServiceProxyModule: EnginePartModule
    {
        public override void Initialize(CPlatformContainer serviceProvider)
        {
            serviceProvider.GetInstances<IServiceProxyFactory>();
        }

        /// <summary>
        /// Inject dependent third-party components
        /// </summary>
        /// <param name="builder"></param>
        protected override void RegisterBuilder(ContainerBuilderWrapper builder)
        {
            base.RegisterBuilder(builder);

        }
        
    }
}
