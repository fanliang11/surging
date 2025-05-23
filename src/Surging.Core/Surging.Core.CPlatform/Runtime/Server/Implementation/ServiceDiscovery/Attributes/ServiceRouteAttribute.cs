using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes
{
   public class ServiceRouteAttribute: Attribute
    {
        public ServiceRouteAttribute(string template)
        {
            Template = template;
        }
         
        public string Template { get; }  
    }
}
