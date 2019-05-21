using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.WS.Configurations
{
    public class BehaviorOption
    {
        public bool IgnoreExtensions { get; set; }

        public bool EmitOnPing { get; set; }

        public string Protocol { get; set; }
    }
}
