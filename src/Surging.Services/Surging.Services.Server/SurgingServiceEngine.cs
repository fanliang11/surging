using Surging.Core.CPlatform.Engines.Implementation;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Surging.Services.Server
{
   public class SurgingServiceEngine: VirtualPathProviderServiceEngine
    {
        public SurgingServiceEngine()
        {
        
            ModuleServiceLocationFormats = new[] {
                EnvironmentHelper.GetEnvironmentVariable("${ModulePath1}|Modules"),
            };
            ComponentServiceLocationFormats  = new[] {
                 EnvironmentHelper.GetEnvironmentVariable("${ComponentPath1}|Components"),
            };
            //ModuleServiceLocationFormats = new[] {
            //   ""
            //};
        }
    }
}
