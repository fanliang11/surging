using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Module
{
    /// <summary>
    /// Defines the <see cref="EnginePartModule" />
    /// </summary>
    public class EnginePartModule : AbstractModule
    {
        #region 方法

        /// <summary>
        /// The Dispose
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
        }

        /// <summary>
        /// The Initialize
        /// </summary>
        /// <param name="context">The context<see cref="AppModuleContext"/></param>
        public override void Initialize(AppModuleContext context)
        {
            base.Initialize(context);
        }

        /// <summary>
        /// The RegisterComponents
        /// </summary>
        /// <param name="builder">The builder<see cref="ContainerBuilderWrapper"/></param>
        internal override void RegisterComponents(ContainerBuilderWrapper builder)
        {
            base.RegisterComponents(builder);
        }

        /// <summary>
        /// The RegisterServiceBuilder
        /// </summary>
        /// <param name="builder">The builder<see cref="IServiceBuilder"/></param>
        protected virtual void RegisterServiceBuilder(IServiceBuilder builder)
        {
        }

        #endregion 方法
    }
}