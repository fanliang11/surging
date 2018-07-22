using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.WS.Runtime
{
    public interface IWSServiceEntryProvider
    {
        IEnumerable<WSServiceEntry> GetEntries();
    }
}
