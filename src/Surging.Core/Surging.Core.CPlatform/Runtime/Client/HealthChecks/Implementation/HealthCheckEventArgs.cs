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
         
        public AddressModel Address { get; private set; }
    }
}
