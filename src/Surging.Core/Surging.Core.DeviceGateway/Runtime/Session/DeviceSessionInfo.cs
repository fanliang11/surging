using Surging.Core.DeviceGateway.Runtime.session;


namespace Surging.Core.DeviceGateway.Runtime.Session
{
    public class DeviceSessionInfo
    {
        public string DeviceId { get; set; }

        public string ServerId { get; set; }

        public string Address { get; set; }

        public long ConnectTime { get; set; }

        public long LastCommTime { get; set; }

        public string MessageTransport { get; set; }
        private String parentDeviceId;

        public DeviceSessionInfo()
        {

        }

        public DeviceSessionInfo(string serverId, IDeviceSession session)
        {
            DeviceSessionInfo sessionInfo = new DeviceSessionInfo();
            sessionInfo.ServerId = serverId;
            sessionInfo.Address = session.GetClientAddress()?.ToString();
            sessionInfo.ConnectTime = session.ConnectTime();
            sessionInfo.DeviceId = session.GetDeviceId();

            //上一次通信时间
            sessionInfo.LastCommTime = session.LastPingTime();

            sessionInfo.MessageTransport = session.GetTransport().ToString();

            //子设备
            //if (session.isWrapFrom(ChildrenDeviceSession.class)) {
            //sessionInfo.setParentDeviceId(session.unwrap(ChildrenDeviceSession.class).getParentDevice().getDeviceId());
        }

    }
}