using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Coap.Runtime
{
    public interface ICoapServiceEntryProvider
    {
        IEnumerable<CoapServiceEntry> GetEntries();
    }
}
