using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Tcp.Runtime
{
    public interface ITcpServiceEntryProvider
    {
        TcpServiceEntry GetEntry();
    }
}
