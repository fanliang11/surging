using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes
{
    public class HttpPostAttribute : HttpMethodAttribute
    {
        private static readonly IEnumerable<string> _supportedMethods = new[] { "POST" };
         
        public HttpPostAttribute()
            : base(_supportedMethods)
        {
        }
         
        public HttpPostAttribute(bool isRegisterMetadata)
            : base(_supportedMethods, isRegisterMetadata)
        {
            
        }
    }
}