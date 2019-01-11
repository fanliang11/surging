using Surging.Core.CPlatform.Address;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Runtime.Client.HealthChecks.Implementation
{
   public class HealthCheckEventArgs
    {
        public HealthCheckEventArgs(AddressModel address)
        {
            Address = address;
        }

        public HealthCheckEventArgs(AddressModel address,bool health)
        {
            Address = address;
            Health = health;
        }

        public AddressModel Address { get; private set; }

        public bool Health { get; private set; }
    }
}
