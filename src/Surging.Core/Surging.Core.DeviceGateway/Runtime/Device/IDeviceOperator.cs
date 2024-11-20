using Surging.Core.CPlatform.Protocol;
using Surging.Core.CPlatform.Transport;
using Surging.Core.DeviceGateway.Runtime.Core;
using Surging.Core.DeviceGateway.Runtime.Core.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Device
{
    public interface IDeviceOperator: IConfigurable
    {
        string GetDeviceId();

    
       string GetNetworkId();
         
        string GetSessionId();

        string GetAddress();

        /**
         * 设置设备地址
         *
         * @param address 地址
         * @return Mono
         */
        void SetAddress(String address);

        /**
         * @param state 状态
         * @see DeviceState#online
         */
        bool PutState(sbyte state);

        /**
         * 获取设备当前缓存的状态,此状态可能与实际的状态不一致.
         *
         * @return 获取当前状态
         * @see DeviceState
         */
        sbyte GetState();

        /**
         * 检查设备的真实状态,此操作将检查设备真实的状态.
         * 如果设备协议中指定了{@link ProtocolSupport#getStateChecker()},则将调用指定的状态检查器进行检查.
         * <br>
         * 默认的状态检查逻辑:
         * <br>
         * <img src="doc-files/device-state-check.svg">
         *
         * @see DeviceStateChecker
         */
        sbyte CheckState();

        /**
         * @return 上线时间
         */
        long GetOnlineTime();

        /**
         * @return 离线时间
         */
        long GetOfflineTime();


        IObservable<bool> Online(string serverId, string sessionId);

        IObservable<bool> Online(string serverId, string sessionId, string address);

        /**
         * 设备上线,通常不需要手动调用
         *
         * @param serverId   设备所在服务ID {@link DeviceSessionManager#getCurrentServerId()}
         * @param address    设备地址
         * @param onlineTime 上线时间 {@link DeviceSession#connectTime()} 大于0有效
         */
        IObservable<bool> Online(string serverId, string address, long onlineTime);

        IObservable<AuthenticationResult> Authenticate(IAuthenticationRequest request);
        bool IsOnline();

        bool Offline();

         
        bool Disconnect();

        IProtocolSupport GetProtocol();

        IMessageSender MessageSender();
    }
}
