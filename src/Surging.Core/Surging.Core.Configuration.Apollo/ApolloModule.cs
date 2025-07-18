using Surging.Core.Configuration.Apollo.Configurations;
using Surging.Core.CPlatform.Module;
using System;

namespace Surging.Core.Configuration.Apollo
{
    public class ApolloModule : EnginePartModule
    { 
        public override void Initialize(AppModuleContext context)
        {
            var serviceProvider = context.ServiceProvoider;
            base.Initialize(context);
            serviceProvider.GetInstances<IConfigurationFactory>().Create();
        }

        /// <summary>
        /// Inject dependent third-party components
        /// </summary>
        /// <param name="builder"></param>
        protected override void RegisterBuilder(ContainerBuilderWrapper builder)
        {
            base.RegisterBuilder(builder);
            builder.RegisterType(typeof(ConfigurationFactory)).As(typeof(IConfigurationFactory));
        }
    }
}
