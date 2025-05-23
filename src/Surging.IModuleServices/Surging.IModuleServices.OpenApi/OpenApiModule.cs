using Surging.Core.CPlatform.Module;
using Surging.Core.KestrelHttpServer;
using Surging.Core.KestrelHttpServer.Extensions;
using Surging.IModuleServices.OpenApi.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.IModuleServices.OpenApi
{
    public class OpenApiModule : KestrelHttpModule
    {
        public override void Initialize(AppModuleContext context)
        {
        }

        public override void RegisterBuilder(WebHostContext context)
        {
        }

        public override void RegisterBuilder(ConfigurationContext context)
        {
            context.Services.AddFilters(typeof(OpenApiFilterAttribute));
        }

        protected override void RegisterBuilder(ContainerBuilderWrapper builder)
        {
        }
    }
}
