using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Surging.Core.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors.Implementation;

namespace Surging.Core.CPlatform.Support
{
    /// <summary>
    /// Defines the <see cref="ServiceCommand" />
    /// </summary>
    public class ServiceCommand
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceCommand"/> class.
        /// </summary>
        public ServiceCommand()
        {
            if (AppConfig.ServerOptions != null)
            {
                FailoverCluster = AppConfig.ServerOptions.FailoverCluster;
                CircuitBreakerForceOpen = AppConfig.ServerOptions.CircuitBreakerForceOpen;
                Strategy = AppConfig.ServerOptions.Strategy;
                ExecutionTimeoutInMilliseconds = AppConfig.ServerOptions.ExecutionTimeoutInMilliseconds;
                RequestCacheEnabled = AppConfig.ServerOptions.RequestCacheEnabled;
                Injection = AppConfig.ServerOptions.Injection;
                InjectionNamespaces = AppConfig.ServerOptions.InjectionNamespaces;
                BreakeErrorThresholdPercentage = AppConfig.ServerOptions.BreakeErrorThresholdPercentage;
                BreakeSleepWindowInMilliseconds = AppConfig.ServerOptions.BreakeSleepWindowInMilliseconds;
                BreakerForceClosed = AppConfig.ServerOptions.BreakerForceClosed;
                BreakerRequestVolumeThreshold = AppConfig.ServerOptions.BreakerRequestVolumeThreshold;
                MaxConcurrentRequests = AppConfig.ServerOptions.MaxConcurrentRequests;
                FallBackName = AppConfig.ServerOptions.FallBackName;
            }
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the BreakeErrorThresholdPercentage
        /// 错误率达到多少开启熔断保护
        /// </summary>
        public int BreakeErrorThresholdPercentage { get; set; } = 50;

        /// <summary>
        /// Gets or sets a value indicating whether BreakerForceClosed
        /// 是否强制关闭熔断
        /// </summary>
        public bool BreakerForceClosed { get; set; }

        /// <summary>
        /// Gets or sets the BreakerRequestVolumeThreshold
        /// 10秒钟内至少多少请求失败，熔断器才发挥起作用
        /// </summary>
        public int BreakerRequestVolumeThreshold { get; set; } = 20;

        /// <summary>
        /// Gets or sets the BreakeSleepWindowInMilliseconds
        /// 熔断多少毫秒后去尝试请求
        /// </summary>
        public int BreakeSleepWindowInMilliseconds { get; set; } = 60000;

        /// <summary>
        /// Gets or sets a value indicating whether CircuitBreakerForceOpen
        /// 是否强制开启熔断
        /// </summary>
        public bool CircuitBreakerForceOpen { get; set; }

        /// <summary>
        /// Gets or sets the ExecutionTimeoutInMilliseconds
        /// 执行超时时间
        /// </summary>
        public int ExecutionTimeoutInMilliseconds { get; set; } = 1000;

        /// <summary>
        /// Gets or sets the FailoverCluster
        /// 故障转移次数
        /// </summary>
        public int FailoverCluster { get; set; } = 3;

        /// <summary>
        /// Gets or sets the FallBackName
        /// IFallbackInvoker 实例名称
        /// </summary>
        public string FallBackName { get; set; }

        /// <summary>
        /// Gets or sets the Injection
        /// 注入
        /// </summary>
        public string Injection { get; set; } = "return null";

        /// <summary>
        /// Gets or sets the InjectionNamespaces
        /// 注入命名空间
        /// </summary>
        public string[] InjectionNamespaces { get; set; }

        /// <summary>
        /// Gets or sets the MaxConcurrentRequests
        /// 信号量最大并发度
        /// </summary>
        public int MaxConcurrentRequests { get; set; } = 200;

        /// <summary>
        /// Gets or sets a value indicating whether RequestCacheEnabled
        /// 是否开启缓存
        /// </summary>
        public bool RequestCacheEnabled { get; set; }

        /// <summary>
        /// Gets or sets the ShuntStrategy
        /// 负载分流策略
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public AddressSelectorMode ShuntStrategy { get; set; } = AddressSelectorMode.Polling;

        /// <summary>
        /// Gets or sets the Strategy
        /// 容错策略
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public StrategyType Strategy { get; set; }

        #endregion 属性
    }
}