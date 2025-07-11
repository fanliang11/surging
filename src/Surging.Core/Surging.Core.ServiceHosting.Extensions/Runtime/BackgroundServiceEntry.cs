using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.ServiceHosting.Extensions.Runtime
{
   public class BackgroundServiceEntry
    {
        public string Path { get; set; }

        public Type Type { get; set; }

        public BackgroundServiceBehavior Behavior { get; set; }
    }
}
