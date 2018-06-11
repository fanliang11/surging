using Surging.Core.CPlatform.Configurations;
using Surging.Core.CPlatform.Configurations.Watch;
using Surging.Core.CPlatform.Runtime.Server;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Routing
{
    public class ServiceRouteWatch : ConfigurationWatch
    {
        private readonly Action _action;
        public ServiceRouteWatch(CPlatformContainer serviceProvider,  Action action)
        {
            this._action = action;
            if (serviceProvider.IsRegistered<IConfigurationWatchManager>())
                serviceProvider.GetInstances<IConfigurationWatchManager>().Register(this);
            _action.Invoke();
        }

        public override async Task Process()
        {
            await Task.Run(_action);
        }

    }
}
