using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.DotNettyWSServer.Runtime
{
   public interface IWSServiceEntryProvider
    {
        IEnumerable<WSServiceEntry> GetEntries();
    }
}
