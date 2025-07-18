using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Transport;
using Surging.Core.DeviceGateway.Runtime.session;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Session
{
    public delegate Task ListenDelegate(DeviceSessionEventArg arg);
    public interface IDeviceSessionManager
    {
        string GetCurrentServerId();
        IObservable<IDeviceSession> AddorUpdate(string deviceId,
                              Func<IDeviceSession, IDeviceSession> func);

        IObservable<IDeviceSession> AddorUpdate(string deviceId,
                            IDeviceSession creator,
                            Func<IDeviceSession, IDeviceSession> updater);


        IObservable<IDeviceSession> GetSession(string deviceId);

        IObservable<IDeviceSession> GetSession(string deviceId, bool unregisterWhenNotAlive);

        ISubject<IDeviceSession> GetSessions();

        /**
         * 移除会话,如果会话存在将触发{@link DeviceSessionEvent}
         * <p>
         * 当设置参数{@code onlyLocal}为true时,将移除整个集群的会话.
         *
         * @param deviceId  设备ID
         * @param onlyLocal 是否只移除本地的会话信息
         * @return 有多少会话被移除
         */
        IObservable<bool> Remove(string deviceId, bool onlyLocal);


        IObservable<bool> IsAlive(string deviceId); 
         
        IObservable<bool> IsAlive(string deviceId, bool onlyLocal);
         
        IObservable<bool> CheckAlive(string deviceId, bool onlyLocal);

     
        IObservable<long> TotalSessions(bool onlyLocal);

        /**
         * 获取全部会话信息
         *
         * @return 会话信息
         */
        ISubject<DeviceSessionInfo> GetSessionInfo();

        /**
         * 获取指定服务的会话信息
         *
         * @param serverId 服务ID
         * @return 会话信息
         */
        ISubject<DeviceSessionInfo> GetSessionInfo(String serverId);

        /**
         * 获取本地的会话信息
         *
         * @return 会话信息
         */
        ISubject<DeviceSessionInfo> GetLocalSessionInfo();

        /**
         * 监听并处理会话事件,可通过调用返回值{@link  Disposable#dispose()}来取消监听
         *
         * @param handler 事件处理器
         * @return Disposable
         */
        event ListenDelegate ListenEvent;
    }
}
