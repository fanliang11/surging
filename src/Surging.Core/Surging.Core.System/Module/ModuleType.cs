using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.System.Module
{
    /// <summary>
    /// 模块类型
    /// </summary>
    public enum ModuleType
    {
        /// <summary>
        /// 接口服务
        /// </summary>
        InterFaceService,
        /// <summary>
        /// 领域
        /// </summary>
        Domain,
        /// <summary>
        /// 仓储
        /// </summary>
        Repository,
        /// <summary>
        /// 模块
        /// </summary>
        Module,
        /// <summary>
        /// 业务模块，包括领域和仓储
        /// </summary>
        BusinessModule,
        /// <summary>
        /// 系统模块，包括InterFaceService和Module
        /// </summary>
        SystemModule,
        /// <summary>
        /// wcf
        /// </summary>
        WcfService,
        /// <summary>
        /// webapi
        /// </summary>
        WebApi
    }
}
