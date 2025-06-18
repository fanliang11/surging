using SuperSocket.ProtoBase;
using Surging.Core.CPlatform.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.SuperSocket
{
    public class SuperSocketPackageInfo  
    {
        public SuperSocketPackageInfo(TransportMessage body)
        { 
            Body = body;
        }
          
        public TransportMessage Body { get; private set; } 
    }
}