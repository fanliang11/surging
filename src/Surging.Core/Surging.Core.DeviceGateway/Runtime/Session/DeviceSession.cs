
using Surging.Core.CPlatform.Codecs.Message; 
using Surging.Core.DeviceGateway.Runtime.Core;
using Surging.Core.DeviceGateway.Runtime.Device;
using System.Net;

namespace Surging.Core.DeviceGateway.Runtime.session
{
    public interface IDeviceSession
    {
        /**
 * @return 会话ID
 */
        string GetId();

        /**
         * @return 设备ID
         */
        string GetDeviceId();

        /**
         * 获取设备操作对象,在类似TCP首次请求的场景下,返回值可能为<code>null</code>.
         * 可以通过判断此返回值是否为<code>null</code>,来处理首次连接的情况。
         *
         * @return void
         */

        IDeviceOperator GetOperator();

        /**
         * @return 最近心跳时间
         */
        long LastPingTime();

        /**
         * @return 创建时间
         */
        long ConnectTime();

        
        IObservable<Task> Send(EncodedMessage encodedMessage);


        MessageTransport GetTransport();

        void Close();

        void Ping();

        bool IsAlive();

        /**
         * 设置close回调
         *
         * @param call 回调
         */
        void OnClose(Action call);

        string GetServerId();

        EndPoint GetClientAddress();
        void KeepAlive();

        void SetKeepAliveTimeout(TimeSpan timeout);

        TimeSpan GetKeepAliveTimeout();

        IObservable<bool> IsAliveAsync();

    }
}
