using Surging.Core.DeviceGateway.Runtime.Enums;
using Surging.Core.DeviceGateway.Runtime.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core
{
    public delegate Task StateListenDelegate(GatewayState arg);
    public interface IDeviceGateway
    {
        string GetId();


        /**
         * 启动网关
         *
         * @return 启动结果
         */
        Task Startup();

        /**
         * 暂停网关,暂停后停止处理设备消息.
         *
         * @return 暂停结果
         */
        Task Stop();

        /**
         * 关闭网关
         *
         * @return 关闭结果
         */
        IObservable<Task> ShutdownAsync();

        bool IsAlive();

        bool IsStarted();

        event StateListenDelegate StateListenEvent;

        Task OnStateChange(GatewayState state);
    }
}
