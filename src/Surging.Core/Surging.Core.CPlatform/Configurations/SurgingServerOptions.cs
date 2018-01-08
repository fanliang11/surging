using Surging.Core.CPlatform.Support;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Configurations
{
    public  class SurgingServerOptions: ServiceCommand
    {
        public string Ip { get; set; }

        public int Port { get; set; }

        public string Token { get; set; } = "True";
    }
}
