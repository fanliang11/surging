using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Network
{
    public class NetworkLogMessage
    {
        public string Id { get; set; }

        public LogLevel logLevel { get; set; }

        public NetworkType NetworkType { get; set; }

        public string EventName { get; set; }

        public string Content { get; set; }

        public DateTime CreateDate { get; set; }
    }
}
