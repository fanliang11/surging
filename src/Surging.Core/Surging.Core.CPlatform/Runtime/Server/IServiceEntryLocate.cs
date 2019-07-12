using Surging.Core.CPlatform.Messages;

namespace Surging.Core.CPlatform.Runtime.Server
{
    #region 接口

    /// <summary>
    /// 一个抽象的服务条目定位器。
    /// </summary>
    public interface IServiceEntryLocate
    {
        #region 方法

        /// <summary>
        /// The Locate
        /// </summary>
        /// <param name="httpMessage">The httpMessage<see cref="HttpMessage"/></param>
        /// <returns>The <see cref="ServiceEntry"/></returns>
        ServiceEntry Locate(HttpMessage httpMessage);

        /// <summary>
        /// 定位服务条目。
        /// </summary>
        /// <param name="invokeMessage">远程调用消息。</param>
        /// <returns>服务条目。</returns>
        ServiceEntry Locate(RemoteInvokeMessage invokeMessage);

        #endregion 方法
    }

    #endregion 接口
}