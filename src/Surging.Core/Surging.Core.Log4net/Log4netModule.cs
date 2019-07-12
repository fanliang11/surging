using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Module;
using Surging.Core.CPlatform.Utilities;

namespace Surging.Core.Log4net
{
    /// <summary>
    /// Defines the <see cref="Log4netModule" />
    /// </summary>
    public class Log4netModule : EnginePartModule
    {
        #region 字段

        /// <summary>
        /// Defines the log4NetConfigFile
        /// </summary>
        private string log4NetConfigFile = "${LogPath}|log4net.config";

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
            var section = CPlatform.AppConfig.GetSection("Logging");
            log4NetConfigFile = EnvironmentHelper.GetEnvironmentVariable(log4NetConfigFile);
            serviceProvider.GetInstances<ILoggerFactory>().AddProvider(new Log4NetProvider(log4NetConfigFile));
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