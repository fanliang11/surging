using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Module;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Nlog
{
    /// <summary>
    /// Defines the <see cref="NLogModule" />
    /// </summary>
    public class NLogModule : EnginePartModule
    {
        #region 字段

        /// <summary>
        /// Defines the nlogConfigFile
        /// </summary>
        private string nlogConfigFile = "${LogPath}|NLog.config";

        #endregion 字段

        #region 方法

        /// <summary>
        /// The Initialize
        /// </summary>
        /// <param name="context">The context<see cref="AppModuleContext"/></param>
        public override void Initialize(AppModuleContext context)
        {
            var serviceProvider = context.ServiceProvoider;
            base.Initialize(context);
            var section = AppConfig.GetSection("Logging");
            nlogConfigFile = EnvironmentHelper.GetEnvironmentVariable(nlogConfigFile);
            NLog.LogManager.LoadConfiguration(nlogConfigFile);
            serviceProvider.GetInstances<ILoggerFactory>().AddProvider(new NLogProvider());
        }

        /// <summary>
        /// Inject dependent third-party components
        /// </summary>
        /// <param name="builder"></param>
        protected override void RegisterBuilder(ContainerBuilderWrapper builder)
        {
            base.RegisterBuilder(builder);
        }

        #endregion 方法
    }
}