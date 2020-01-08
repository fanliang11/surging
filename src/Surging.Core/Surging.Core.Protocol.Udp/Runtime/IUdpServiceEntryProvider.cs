using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Udp.Runtime
{
    public interface IUdpServiceEntryProvider
    {
        UdpServiceEntry GetEntry();
    }
}
