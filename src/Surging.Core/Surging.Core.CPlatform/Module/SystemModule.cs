using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Module
{
    /// <summary>
    /// 系统模块基类
    /// </summary>
    public class SystemModule : AbstractModule
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
        /// The RegisterComponents
        /// </summary>
        /// <param name="builder">The builder<see cref="ContainerBuilderWrapper"/></param>
        internal override void RegisterComponents(ContainerBuilderWrapper builder)
        {
            base.RegisterComponents(builder);
        }

        #endregion 方法
    }
}