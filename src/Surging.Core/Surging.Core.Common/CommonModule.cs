using Surging.Core.CPlatform.Module;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Common
{
    public  class CommonModule:SystemModule
    {
        public override void Initialize(AppModuleContext context)
        {
            base.Initialize(context);
        }

        protected override void RegisterBuilder(ContainerBuilderWrapper builder)
        {
            base.RegisterBuilder(builder);
        }
    }
}
