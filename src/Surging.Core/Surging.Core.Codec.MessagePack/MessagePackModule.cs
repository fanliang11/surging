using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Module;
using Surging.Core.CPlatform.Transport.Codec;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Codec.MessagePack
{
    /// <summary>
    /// Defines the <see cref="MessagePackModule" />
    /// </summary>
    public class MessagePackModule : EnginePartModule
    {
        #region 方法

        /// <summary>
        /// The Initialize
        /// </summary>
        /// <param name="context">The context<see cref="AppModuleContext"/></param>
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
            builder.RegisterType<MessagePackTransportMessageCodecFactory>().As<ITransportMessageCodecFactory>().SingleInstance();
        }

        #endregion 方法
    }
}