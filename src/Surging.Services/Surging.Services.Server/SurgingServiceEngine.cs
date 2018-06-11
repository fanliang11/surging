using Surging.Core.CPlatform.Engines.Implementation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Services.Server
{
   public class SurgingServiceEngine: VirtualPathProviderServiceEngine
    {
        public SurgingServiceEngine()
        {
            ModuleServiceLocationFormats = new[] {
                @"Modules",
                 @"Modules\2.1",
                   @"Modules\1.0",
                  @"Modules\User\0.0.1"
            };
            ComponentServiceLocationFormats  = new[] {
                @"components",
                 @"components\mongodb",
                   @"components\1.0",
                  @"Modules\User\0.0.1"
            };
            //ModuleServiceLocationFormats = new[] {
            //   ""
            //};
        }
    }
}
