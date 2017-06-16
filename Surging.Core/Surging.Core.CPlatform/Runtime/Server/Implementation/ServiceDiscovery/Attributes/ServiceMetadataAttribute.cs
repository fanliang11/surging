using System;

namespace Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes
{
    /// <summary>
    /// 服务元数据标记。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class ServiceMetadataAttribute : ServiceDescriptorAttribute
    {
        /// <summary>
        /// 初始化构造函数。
        /// </summary>
        /// <param name="name">名称。</param>
        /// <param name="data">数据。</param>
        public ServiceMetadataAttribute(string name, object data)
        {
            Name = name;
            Data = data;
        }

        /// <summary>
        /// 名称。
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 数据。
        /// </summary>
        public object Data { get; }

        #region Overrides of ServiceDescriptorAttribute

        /// <summary>
        /// 应用标记。
        /// </summary>
        /// <param name="descriptor">服务描述符。</param>
        public override void Apply(ServiceDescriptor descriptor)
        {
            descriptor.Metadatas[Name] = Data;
        }

        #endregion Overrides of RpcServiceDescriptorAttribute
    }
}