using System;

namespace Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes
{
    /// <summary>
    /// 服务标记。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class ServiceAttribute : ServiceDescriptorAttribute
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceAttribute"/> class.
        /// </summary>
        public ServiceAttribute()
        {
            IsWaitExecution = true;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the Date
        /// 日期
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// Gets or sets the Director
        /// 负责人
        /// </summary>
        public string Director { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether DisableNetwork
        /// 是否禁用外网访问
        /// </summary>
        public bool DisableNetwork { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether EnableAuthorization
        /// 是否授权
        /// </summary>
        public bool EnableAuthorization { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsWaitExecution
        /// 是否需要等待服务执行。
        /// </summary>
        public bool IsWaitExecution { get; set; }

        /// <summary>
        /// Gets or sets the Name
        /// 名称
        /// </summary>
        public string Name { get; set; }

        #endregion 属性

        #region 方法

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

        #endregion 方法
    }
}