using System;

namespace Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes
{
    /// <summary>
    /// 服务集标记。
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class ServiceBundleAttribute : Attribute
    {
        public ServiceBundleAttribute(string routeTemplate,bool isPrefix=true)
        {
            RouteTemplate = routeTemplate;
            IsPrefix = isPrefix;
        }
        public string RouteTemplate { get; }

        public bool IsPrefix { get; }
    }
}