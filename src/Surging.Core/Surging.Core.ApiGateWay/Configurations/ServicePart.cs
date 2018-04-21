using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.ApiGateWay.Configurations
{
   public class ServicePart
    {
        public string MainPath { get; set; } = "part/service/aggregation";

       public Services Services { get; set; }
    }
}
