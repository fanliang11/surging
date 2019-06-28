using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Module
{
   public class SystemModule : AbstractModule
    {
        public override void Initialize(AppModuleContext context)
        {
            base.Initialize(context);
        }
        
        internal override void RegisterComponents(ContainerBuilderWrapper builder)
        {
            base.RegisterComponents(builder);
        }
    }
}

