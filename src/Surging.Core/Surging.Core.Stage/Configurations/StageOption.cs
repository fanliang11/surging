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

        public string HttpPorts { get;  set; }
    }
}
