using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.DNS.Runtime
{
    public interface IDnsServiceEntryProvider
    {
        DnsServiceEntry GetEntry();
    }
}
