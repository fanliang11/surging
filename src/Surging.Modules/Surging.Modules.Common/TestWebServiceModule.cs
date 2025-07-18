using Microsoft.Extensions.DependencyInjection;
using Surging.Core.CPlatform.Module;
using Surging.Core.KestrelHttpServer;
using Surging.IModuleServices.Common;
using Surging.Modules.Common.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Modules.Common
{
    public class TestWebServiceModule : KestrelHttpModule
    {
        public override void Initialize(AppModuleContext context)
        {

        }

        public override void Initialize(ApplicationInitializationContext builder)
        {
        }
        public override void RegisterBuilder(ConfigurationContext context)
        {
            context.Services.AddSingleton<IWebServiceService, WebServiceService>();
        }

        protected async override void RegisterBuilder(ContainerBuilderWrapper builder)
        {
        }
    }
}
