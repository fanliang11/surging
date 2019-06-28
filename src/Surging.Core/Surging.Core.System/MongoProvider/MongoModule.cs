using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Module;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.System.MongoProvider.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.System.MongoProvider
{
    public class MongoModule : SystemModule
    {
        /// <summary>
        ///  Function module initialization,trigger when the module starts loading
        /// </summary>
        public override void Initialize(AppModuleContext context)
        {
            base.Initialize(context);
        }

        /// <summary>
        /// Inject dependent third-party components
        /// </summary>
        /// <param name="builder"></param>
        protected override void RegisterBuilder(ContainerBuilderWrapper builder)
        {
            base.RegisterBuilder(builder);
            builder.RegisterGeneric(typeof(MongoRepository<>)).As(typeof(IMongoRepository<>)).SingleInstance();
        }
    }
}
