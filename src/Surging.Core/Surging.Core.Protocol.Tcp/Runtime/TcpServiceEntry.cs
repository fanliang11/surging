using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Tcp.Runtime
{
    public class TcpServiceEntry
    {
        public string Path { get; set; }

        public Type Type { get; set; }


        public Func<TcpBehavior> Behavior { get; set; }
    }
}
