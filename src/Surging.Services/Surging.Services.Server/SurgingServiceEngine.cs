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
                 @"Modules/2.1"
            }; 
        }
    }
}
