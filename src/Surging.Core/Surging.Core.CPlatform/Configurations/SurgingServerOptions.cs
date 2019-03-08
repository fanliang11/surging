using Surging.Core.CPlatform.Support;
using System;
using System.Collections.Generic;
using System.Net;

namespace Surging.Core.CPlatform.Configurations
{
    public  partial class SurgingServerOptions: ServiceCommand
    {
        public string Ip { get; set; }

        public string MappingIP { get; set; }

        public int MappingPort { get; set; }

        public string WanIp { get; set; }

        public bool IsModulePerLifetimeScope { get; set; }

        public double WatchInterval { get; set; } = 20d;

        public int DisconnTimeInterval { get; set; } = 60;

        public bool Libuv { get; set; } = false;

        public int SoBacklog { get; set; } = 8192;

        public bool EnableRouteWatch { get; set; }

        public IPEndPoint IpEndpoint { get; set; }

        public List<ModulePackage> Packages { get; set; } = new List<ModulePackage>();

        public CommunicationProtocol Protocol { get; set; }
        public string RootPath { get; set; }

        public string WebRootPath { get; set; } = AppContext.BaseDirectory;

        public int Port { get; set; }

        public bool ReloadOnChange { get; set; } = false;

        public ProtocolPortOptions Ports { get; set; } = new  ProtocolPortOptions();

        public string Token { get; set; } = "True";

        public string NotRelatedAssemblyFiles { get; set; }

        public string RelatedAssemblyFiles { get; set; } = "";
    }
}
