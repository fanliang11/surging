using Surging.Core.CPlatform.Utilities;
using Surging.Core.DeviceGateway.Runtime.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core.Implementation
{
    public abstract class AbstractDeviceGateway : IDeviceGateway
    {
        private  GatewayState _gatewayState;
        private readonly string _id;

        public AbstractDeviceGateway(String id)
        {
            _id = id;
        }

        public event StateListenDelegate StateListenEvent;

        public string GetId()
        {
            return _id;
        }

        public bool IsAlive()
        {
            return _gatewayState == GatewayState.Started || _gatewayState == GatewayState.Starting;
        }

        public bool IsStarted()
        {
            return _gatewayState == GatewayState.Started;
        }

        public async Task OnStateChange(GatewayState state)
        {
                _gatewayState = state;
            if (StateListenEvent == null)
                return;
            await StateListenEvent(state);
        }

        public   IObservable<Task> ShutdownAsync()
        {
            var shutdownHandle = DoShutdown();
                shutdownHandle.Subscribe(async p =>
            {
                await OnStateChange(GatewayState.Shutdown);
            }); 
            return shutdownHandle;
        }

        protected abstract IObservable<Task> DoShutdown();

        protected abstract IObservable<Task> DoStartup();

        public async Task Startup()
        {
            if (_gatewayState == GatewayState.Stop)
            {
                await OnStateChange(GatewayState.Started);
                return;
            }
            if (_gatewayState == GatewayState.Started || _gatewayState == GatewayState.Starting)
            {
                return;
            }
            await OnStateChange(GatewayState.Starting);
            this.DoStartup()
            .Subscribe(async (p) => await OnStateChange(GatewayState.Started));
        }

        public async Task Stop()
        {
            await OnStateChange(GatewayState.Stop); 
        }
    }
}
