using System;

namespace Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes
{
    /// <summary>
    /// 服务标记。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class ServiceAttribute : ServiceDescriptorAttribute
    {
        /// <summary>
        /// 初始化构造函数
        /// </summary>
        public ServiceAttribute()
        {
            IsWaitExecution = true;
        }

        /// <summary>
        /// 是否需要等待服务执行。
        /// </summary>
        public bool IsWaitExecution { get; set; }

        /// <summary>
        /// 负责人
        /// </summary>
        public string Director { get; set; }

        /// <summary>
        /// 是否禁用外网访问
        /// </summary>
        public bool DisableNetwork { get; set; }

        /// <summary>
        /// 是否授权
        /// </summary>
        public bool EnableAuthorization { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 日期
        /// </summary>
        public string Date { get; set; }
        #region Overrides of DescriptorAttribute

        /// <summary>
        /// 应用标记。
        /// </summary>
        /// <param name="descriptor">服务描述符。</param>
        public override void Apply(ServiceDescriptor descriptor)
        {
            descriptor
                .WaitExecution(IsWaitExecution)
                .EnableAuthorization(EnableAuthorization)
                .DisableNetwork(DisableNetwork)
                .Director(Director)
                .GroupName(Name)
                .Date(Date);
        }

        #endregion Overrides of ServiceDescriptorAttribute
    }
}