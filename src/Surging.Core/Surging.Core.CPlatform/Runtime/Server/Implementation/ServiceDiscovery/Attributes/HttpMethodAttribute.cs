using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public abstract class HttpMethodAttribute : Attribute
    { 

        public HttpMethodAttribute(IEnumerable<string> httpMethods)
            : this(httpMethods, false)
        {
        }

        public HttpMethodAttribute(IEnumerable<string> httpMethods,bool isRegisterMetadata)
        {
            if (httpMethods == null)
            {
                throw new ArgumentNullException(nameof(httpMethods));
            }

            HttpMethods = httpMethods;
            IsRegisterMetadata = isRegisterMetadata;
        } 
        public IEnumerable<string> HttpMethods { get; }
        public bool IsRegisterMetadata { get; }

    }
}
