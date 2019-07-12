using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Surging.Core.CPlatform.Module;
using Surging.Core.KestrelHttpServer;
using Surging.Core.KestrelHttpServer.Filters;
using Surging.Core.Stage.Configurations;
using Surging.Core.Stage.Internal;
using Surging.Core.Stage.Internal.Implementation;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Surging.Core.Stage
{
    /// <summary>
    /// Defines the <see cref="StageModule" />
    /// </summary>
    public class StageModule : KestrelHttpModule
    {
        #region 字段

        /// <summary>
        /// Defines the _listener
        /// </summary>
        private IWebServerListener _listener;

        #endregion 字段

        #region 方法

        /// <summary>
        /// The Initialize
        /// </summary>
        /// <param name="context">The context<see cref="ApplicationInitializationContext"/></param>
        public override void Initialize(ApplicationInitializationContext context)
        {
        }

        /// <summary>
        /// The Initialize
        /// </summary>
        /// <param name="context">The context<see cref="AppModuleContext"/></param>
        public override void Initialize(AppModuleContext context)
        {
            _listener = context.ServiceProvoider.GetInstances<IWebServerListener>();
        }

        /// <summary>
        /// The RegisterBuilder
        /// </summary>
        /// <param name="context">The context<see cref="ConfigurationContext"/></param>
        public override void RegisterBuilder(ConfigurationContext context)
        {
        }

        /// <summary>
        /// The RegisterBuilder
        /// </summary>
        /// <param name="context">The context<see cref="WebHostContext"/></param>
        public override void RegisterBuilder(WebHostContext context)
        {
            _listener.Listen(context);
        }

        /// <summary>
        /// The RegisterBuilder
        /// </summary>
        /// <param name="builder">The builder<see cref="ContainerBuilderWrapper"/></param>
        protected override void RegisterBuilder(ContainerBuilderWrapper builder)
        {
            var section = CPlatform.AppConfig.GetSection("Stage");
            if (section.Exists())
            {
                AppConfig.Options = section.Get<StageOption>();
            }
            builder.RegisterType<WebServerListener>().As<IWebServerListener>().SingleInstance();
            builder.RegisterType<AuthorizationFilterAttribute>().As<IAuthorizationFilter>().SingleInstance();
        }

        #endregion 方法
    }
}