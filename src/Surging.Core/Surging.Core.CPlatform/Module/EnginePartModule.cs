using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Module
{
    public class EnginePartModule : AbstractModule
    {
        public override void Initialize(AppModuleContext context)
        {
            base.Initialize(context);
        }

        public override void Dispose()
        {
            base.Dispose();
        }


        protected virtual void RegisterServiceBuilder(IServiceBuilder builder)
        {
        }


        internal override void RegisterComponents(ContainerBuilderWrapper builder)
        {
            base.RegisterComponents(builder);
          
        }
        
    }

}