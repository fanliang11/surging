namespace Surging.Core.CPlatform.Support
{
    /// <summary>
    /// 容错策略
    /// </summary>
    public enum StrategyType
    {
        /// <summary>
        /// 故障转移策略、失败切换远程服务机制
        /// </summary>
        Failover = 0,
        /// <summary>
        /// 脚本注入策略、失败执行注入脚本
        /// </summary>
        Injection = 1,
        /// <summary>
        /// 回退策略、失败时调用通过FallBackName指定的接口
        /// </summary>
        FallBack = 2,
    }
}
