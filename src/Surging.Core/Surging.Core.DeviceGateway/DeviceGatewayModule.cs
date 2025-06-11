using Surging.Core.CPlatform.Module;
using Surging.Core.CPlatform.Network;
using Surging.Core.DeviceGateway.Runtime.Core;
using Surging.Core.DeviceGateway.Runtime.Core.Implementation;
using Surging.Core.DeviceGateway.Runtime.Device;
using Surging.Core.DeviceGateway.Runtime.Device.Implementation;
using Surging.Core.DeviceGateway.Runtime.Session;
using Surging.Core.DeviceGateway.Runtime.Session.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway
{
    public class DeviceGatewayModule:EnginePartModule
    {
        public override void Initialize(AppModuleContext context)
        {
            base.Initialize(context);
        }


        protected override void RegisterBuilder(ContainerBuilderWrapper builder)
        {
            base.RegisterBuilder(builder); 
            builder.RegisterType(typeof(TcpServerDeviceGatewayProvider)).Named(MessageTransport.Tcp.ToString(), typeof(IDeviceGatewayProvider)).SingleInstance();
            builder.RegisterType(typeof(DefaultDeviceGatewayManage)).As(typeof(IDeviceGatewayManage)).SingleInstance();
            builder.RegisterType(typeof(DefaultDeviceSessionManager)).As(typeof(IDeviceSessionManager)).SingleInstance();
            builder.RegisterType(typeof(DefaultDeviceRegistry)).As(typeof(IDeviceRegistry)).SingleInstance();
        }
    } 
}
