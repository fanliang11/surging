using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Module
{
   public class SystemModule : AbstractModule
    {
        public override void Initialize(CPlatformContainer serviceProvider)
        {
            base.Initialize(serviceProvider);
        }
        
        internal override void RegisterComponents(ContainerBuilderWrapper builder)
        {
            base.RegisterComponents(builder);
        }
    }
}

