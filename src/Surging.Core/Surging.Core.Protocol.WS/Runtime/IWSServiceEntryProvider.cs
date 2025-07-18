using System;
using System.Collections.Generic;
using System.Text;
using WebSocketCore.Server;

namespace Surging.Core.Protocol.WS.Runtime
{
    public interface IWSServiceEntryProvider
    {
        IEnumerable<WSServiceEntry> GetEntries();
        void ChangeEntry(string path, Func<WebSocketSessionManager> client);
    }
}
