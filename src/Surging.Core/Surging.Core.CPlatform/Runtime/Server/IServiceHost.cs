using System;
using System.Net;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Runtime.Server
{
    #region 接口

    /// <summary>
    /// 一个抽象的服务主机。
    /// </summary>
    public interface IServiceHost : IDisposable
    {
        #region 方法

        /// <summary>
        /// 启动主机。
        /// </summary>
        /// <param name="endPoint">主机终结点。</param>
        /// <returns>一个任务。</returns>
        Task StartAsync(EndPoint endPoint);

        /// <summary>
        /// 启动主机。
        /// </summary>
        /// <param name="ip">The ip<see cref="string"/></param>
        /// <param name="port">The port<see cref="int"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task StartAsync(string ip, int port);

        #endregion 方法
    }

    #endregion 接口
}