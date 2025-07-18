using Microsoft.AspNetCore.Server.Kestrel.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Stage.Configurations
{
   public class StageOption
    {
        public bool EnableHttps { get;  set; } 
   
        public string CertificateFileName { get;  set; }

        public string CertificateLocation { get; set; }

        public string CertificatePassword { get;  set; }

        public string HttpsPort { get;  set; }

        public bool IsCamelCaseResolver { get; set; }

        public ApiGetwayOption ApiGetWay { get; set; }

        public HttpProtocols Protocols { get; set; } = HttpProtocols.Http1AndHttp2;

        public DataRateOption MinRequestBodyDataRate { get; set; }

        public DataRateOption MinResponseDataRate { get; set; }

        public long? MaxRequestBodySize { get; set; }

        public long? MaxConcurrentConnections { get; set; }

        public long? MaxConcurrentUpgradedConnections { get; set; }

        public long? MaxRequestBufferSize { get; set; }

        public int MaxRequestHeaderCount { get; set; } = 100;

        public int MaxRequestHeadersTotalSize { get; set; } = 32768;

        public int MaxRequestLineSize { get; set; } = 8192;

        public long? MaxResponseBufferSize { get; set; }

        public AccessPolicyOption AccessPolicy { get; set; }

        public List<AccessSettingOption> AccessSetting { get; set; }

        public string HttpPorts { get;  set; }
    }
}
