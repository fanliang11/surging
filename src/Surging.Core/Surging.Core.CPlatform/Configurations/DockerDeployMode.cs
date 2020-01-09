using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Configurations
{
    /// <summary>
    /// Docker容器部署模式
    /// </summary>
    public enum DockerDeployMode
    {
        /// <summary>
        /// 正常模式
        /// </summary>
        Standard,

        /// <summary>
        /// Swarm集群模式
        /// </summary>
        Swarm
    }
}
