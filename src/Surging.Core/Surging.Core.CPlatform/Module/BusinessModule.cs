using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Module
{
    /// <summary>
    ///  业务模块基类
    /// </summary>
    public class BusinessModule : AbstractModule
    {
        public override void Initialize()
        {
            base.Initialize();
        }
        
        internal override void RegisterComponents(ContainerBuilderWrapper builder)
        {
            base.RegisterComponents(builder);
        }
    }
}
