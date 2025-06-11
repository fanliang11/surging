using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes
{
    public class HttpOptionsAttribute : HttpMethodAttribute
    {
        private static readonly IEnumerable<string> _supportedMethods = new[] { "OPTIONS" };

        public HttpOptionsAttribute()
            : base(_supportedMethods)
        {
        }

        public HttpOptionsAttribute(bool isRegisterMetadata)
            : base(_supportedMethods, isRegisterMetadata)
        {
        }
    }
}
