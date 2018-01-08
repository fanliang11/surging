using System;

namespace Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes
{
    /// <summary>
    /// 服务描述符标记。
    /// </summary>
    public abstract class ServiceDescriptorAttribute : Attribute
    {
        /// <summary>
        /// 应用标记。
        /// </summary>
        /// <param name="descriptor">服务描述符。</param>
        public abstract void Apply(ServiceDescriptor descriptor);
    }
}