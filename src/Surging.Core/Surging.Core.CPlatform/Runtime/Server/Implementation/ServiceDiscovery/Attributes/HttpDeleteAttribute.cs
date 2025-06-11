using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes
{
   public class HttpDeleteAttribute : HttpMethodAttribute
    {
        private static readonly IEnumerable<string> _supportedMethods = new[] { "DELETE" };

        public HttpDeleteAttribute()
            : base(_supportedMethods)
        {
        }

        public HttpDeleteAttribute(bool isRegisterMetadata)
            : base(_supportedMethods,isRegisterMetadata)
        {
          
        }
    }
}
