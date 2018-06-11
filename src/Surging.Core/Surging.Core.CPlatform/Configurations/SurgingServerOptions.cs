using Surging.Core.CPlatform.Support;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Surging.Core.CPlatform.Configurations
{
    public  partial class SurgingServerOptions: ServiceCommand
    {
        public string Ip { get; set; }

        public string MappingIP { get; set; }

        public int MappingPort { get; set; }

        public double WatchInterval { get; set; } = 20d;

        public IPEndPoint IpEndpoint { get; set; }

        public string RootPath { get; set; }

        public int Port { get; set; }

        public string Token { get; set; } = "True";

        public string NotRelatedAssemblyFiles { get; set; }

        public string RelatedAssemblyFiles { get; set; } = "";
    }
}
