using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Surging.Core.KestrelHttpServer
{
    public class WebHostContext
    {
        public WebHostContext(WebHostBuilderContext context, KestrelServerOptions options, IPAddress ipAddress)
        {
            WebHostBuilderContext = Check.NotNull(context, nameof(context));
            KestrelOptions = Check.NotNull(options, nameof(options));
            Address = ipAddress;
        }

        public WebHostBuilderContext WebHostBuilderContext { get; }

        public KestrelServerOptions KestrelOptions { get; }

        public IPAddress Address { get; }

    }
}
