using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Consul
{
   public  class EnvConfig
    {
        public static string ConsulConn {

            get
            {
                return EnvironmentHelper.GetEnvironmentVariable(EnvironmentName.ConsulConn);
            }
        }
    }
}
