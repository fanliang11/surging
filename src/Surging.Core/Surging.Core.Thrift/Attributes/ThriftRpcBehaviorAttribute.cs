using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Thrift.Attributes
{
   public class ThriftRpcBehaviorAttribute :Attribute
    {
        public int MaxMessageSize { get; set; }  
        public int MaxFrameSize { get; set; }  
        public int RecursionLimit { get; set; }

    }
}
