using DotNetty.Common.Utilities;
using Surging.Core.CPlatform.Network;
using Surging.Core.CPlatform.Protocol;
using Surging.Core.DeviceGateway.Runtime.Device;
using Surging.Core.DeviceGateway.Runtime.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core.Implementation
{
    public class TcpDeviceGateway : AbstractDeviceGateway
    {
        public INetwork Network { get; set; }
        public IObservable<IProtocolSupport> Protocol { get; set; }

        private readonly IDeviceRegistry _registry;

        private readonly IDeviceSessionManager _sessionManager;

        public TcpDeviceGateway(string id, IObservable<IProtocolSupport> protocol, IDeviceRegistry registry, IDeviceSessionManager sessionManager, INetwork network) : base(id)
        {
            Protocol = protocol;
            _registry = registry;
            _sessionManager = sessionManager;
            Network = network;
        }

        protected override IObservable<Task> DoShutdown()
        {
            Network.Shutdown();
            return Observable.Return(Task.CompletedTask);
        }

        protected override IObservable<Task> DoStartup()
        {
            return Observable.Return(Network.StartAsync());
        }
    }
}
