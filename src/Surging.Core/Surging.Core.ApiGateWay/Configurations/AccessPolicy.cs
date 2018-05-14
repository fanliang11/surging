using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.ApiGateWay.Configurations
{
   public class AccessPolicy
    {
        public string[] Origins { get; set; }

        public bool AllowAnyHeader { get; set; }

        public bool AllowAnyMethod { get; set; }

        public bool AllowAnyOrigin { get; set; }

        public bool AllowCredentials { get; set; }
    }
}
