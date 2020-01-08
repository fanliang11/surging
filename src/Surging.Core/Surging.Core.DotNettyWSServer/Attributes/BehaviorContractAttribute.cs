using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.DotNettyWSServer.Attributes
{
    public class BehaviorContractAttribute: Attribute
    {

        public string Protocol { get; set; }
    }
}
