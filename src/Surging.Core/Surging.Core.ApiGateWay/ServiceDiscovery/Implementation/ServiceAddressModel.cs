using Surging.Core.CPlatform.Address;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.ApiGateWay.ServiceDiscovery.Implementation
{
    public class ServiceAddressModel 
    {
        public AddressModel Address { get; set; }

        public  bool IsHealth { get; set; }
    }
}
