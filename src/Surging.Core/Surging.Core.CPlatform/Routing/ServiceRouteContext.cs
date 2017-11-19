using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Routing.Implementation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Routing
{
    public class ServiceRouteContext 
    { 
        public ServiceRoute  Route { get; set; }

        public RemoteInvokeResultMessage ResultMessage { get; set; }

        public RemoteInvokeMessage InvokeMessage { get; set; }
    }
}
