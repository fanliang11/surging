using System;

namespace Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes
{
    /// <summary>
    /// 服务集标记。
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class ServiceBundleAttribute : Attribute
    {
        public ServiceBundleAttribute(string routeTemplate)
        {
            RouteTemplate = routeTemplate;
        }
        public string RouteTemplate { get; }
    }
}