using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Runtime.Client.Implementation
{
    public class ServiceMonitorWatcher : IServiceMonitorWatcher
    {
        private readonly IServiceRouteManager _serviceRouteManager;

        private readonly IServiceCommandManager _serviceCommandManager;
        public ServiceMonitorWatcher(IServiceRouteManager serviceRouteManager, IServiceCommandManager serviceCommandManager) 
        {
            _serviceRouteManager = serviceRouteManager;
             _serviceCommandManager = serviceCommandManager;
        }
        public async ValueTask AddNodeMonitorWatcher(string serviceId)
        {
           await _serviceRouteManager.AddNodeMonitorWatcher(serviceId);
           await _serviceCommandManager.AddNodeMonitorWatcher(serviceId);
        }
    }
}
