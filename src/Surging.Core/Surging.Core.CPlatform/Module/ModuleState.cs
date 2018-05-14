using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Module
{
    /// <summary>
    /// 模块状态枚举。
    /// </summary>
    public enum ModuleState
    {
        /// <summary>
        /// 已安装。
        /// </summary>
        Installed,

        /// <summary>
        /// 启动。
        /// </summary>
        Start,

        /// <summary>
        /// 停止。
        /// </summary>
        Stop,

        /// <summary>
        /// 已卸载。
        /// </summary>
        Uninstalled
    }
}
