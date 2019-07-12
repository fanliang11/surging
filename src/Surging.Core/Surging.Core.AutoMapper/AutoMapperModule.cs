using Microsoft.Extensions.Configuration;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Module;
using CPlatformAppConfig = Surging.Core.CPlatform.AppConfig;

namespace Surging.Core.AutoMapper
{
    /// <summary>
    /// Defines the <see cref="AutoMapperModule" />
    /// </summary>
    public class AutoMapperModule : EnginePartModule
    {
        #region 方法

        /// <summary>
        /// The Initialize
        /// </summary>
        /// <param name="context">The context<see cref="AppModuleContext"/></param>
        public override void Initialize(AppModuleContext context)
        {
            base.Initialize(context);
            context.ServiceProvoider.GetInstances<IAutoMapperBootstrap>().Initialize();
        }

        /// <summary>
        /// The RegisterBuilder
        /// </summary>
        /// <param name="builder">The builder<see cref="ContainerBuilderWrapper"/></param>
        protected override void RegisterBuilder(ContainerBuilderWrapper builder)
        {
            var configAssembliesStr = CPlatformAppConfig.GetSection("Automapper:Assemblies").Get<string>();
            if (!string.IsNullOrEmpty(configAssembliesStr))
            {
                AppConfig.AssembliesStrings = configAssembliesStr.Split(";");
            }
            builder.RegisterType<AutoMapperBootstrap>().As<IAutoMapperBootstrap>();
        }

        #endregion 方法
    }
}