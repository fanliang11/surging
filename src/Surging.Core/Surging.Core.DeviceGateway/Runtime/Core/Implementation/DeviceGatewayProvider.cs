using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core.Implementation
{
    public abstract class DeviceGatewayProvider : IDeviceGatewayProvider
    {
        public abstract IObservable<IDeviceGateway> CreateDeviceGateway(DeviceGatewayProperties properties);
      

        public virtual string GetChannel()
        {
           return "Network";
        }

        public virtual string GetDescription()
        {
           return "";
        }


        public abstract string GetId();

        public abstract string GetName();

        public abstract MessageTransport GetTransport();

        public abstract IObservable<IDeviceGateway> ReloadDeviceGateway(IDeviceGateway gateway, DeviceGatewayProperties properties);
    }
}
