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
        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void RegisterBuilder(ContainerBuilderWrapper builder)
        {
            base.RegisterBuilder(builder);
            builder.RegisterGeneric(typeof(MongoRepository<>)).As(typeof(IMongoRepository<>)).SingleInstance();
        }
    }
}
