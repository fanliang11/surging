using Microsoft.Extensions.Configuration;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Module;
using CPlatformAppConfig = Surging.Core.CPlatform.AppConfig;

namespace Surging.Core.AutoMapper
{
    public class AutoMapperModule : EnginePartModule
    {

        public override void Initialize(AppModuleContext context)
        {
            base.Initialize(context);
            context.ServiceProvoider.GetInstances<IAutoMapperBootstrap>().Initialize();
        }

        protected override void RegisterBuilder(ContainerBuilderWrapper builder)
        {
            var configAssembliesStr = CPlatformAppConfig.GetSection("Automapper:Assemblies").Get<string>();
            if (!string.IsNullOrEmpty(configAssembliesStr))
            {
                AppConfig.AssembliesStrings = configAssembliesStr.Split(";");
            }
            builder.RegisterType<AutoMapperBootstrap>().As<IAutoMapperBootstrap>();
        }


    }
}
