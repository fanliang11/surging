using System;

namespace Surging.Core.CPlatform.Support
{
    /// <summary>
    /// Defines the <see cref="ServiceInvokeListenInfo" />
    /// </summary>
    public class ServiceInvokeListenInfo
    {
        #region 属性

        /// <summary>
        /// Gets or sets the ConcurrentRequests
        /// 并发数
        /// </summary>
        public int ConcurrentRequests { get; set; }

        /// <summary>
        /// Gets or sets the FaultRemoteServiceRequests
        /// 失败调用请求数
        /// </summary>
        public int FaultRemoteServiceRequests { get; set; }

        /// <summary>
        /// Gets or sets the FinalRemoteInvokeTime
        /// 最后一次远程调用时间
        /// </summary>
        public DateTime FinalRemoteInvokeTime { get; set; }

        /// <summary>
        /// Gets or sets the FirstInvokeTime
        /// 首次调用时间
        /// </summary>
        public DateTime FirstInvokeTime { get; set; }

        /// <summary>
        /// Gets or sets the LocalServiceRequests
        /// 本地调用请求数
        /// </summary>
        public int LocalServiceRequests { get; set; }

        /// <summary>
        /// Gets or sets the RemoteServiceRequests
        /// 远程调用请求数
        /// </summary>
        public int? RemoteServiceRequests { get; set; }

        /// <summary>
        /// Gets or sets the SinceFaultRemoteServiceRequests
        /// 距上次失败调用次数
        /// </summary>
        public int SinceFaultRemoteServiceRequests { get; set; }

        #endregion 属性
    }
}