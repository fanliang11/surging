using Surging.Core.Protocol.WS.Configurations;
using System;
using System.Collections.Generic;
using System.Text;
using WebSocketCore.Server;

namespace Surging.Core.Protocol.WS.Runtime
{
   public  class WSServiceEntry
    {
        public string Path { get; set; }

        public Type Type { get; set; }

        public  WebSocketBehavior Behavior { get; set; }

        public  Func<WebSocketBehavior> FuncBehavior { get; set; }
    }
}
