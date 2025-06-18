using Surging.Core.DeviceGateway.Runtime.session;
using Surging.Core.DeviceGateway.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Session
{
    public class DeviceSessionEventArg
    {
        public long Timestamp {  get; set; }

        //事件类型
        public DeviceSessionEventArgType  EventArgType {  get; set; }

        //会话
        public IDeviceSession Session { get;set; }


        public DeviceSessionEventArg(DeviceSessionEventArgType eventArgType, IDeviceSession session)
        {
            EventArgType = eventArgType;
            Session = session;
            Timestamp = Utility.CurrentTimeMillis();
        }

        public enum DeviceSessionEventArgType
        {
            //注册
            Register,
            //注销
            Unregister
        }
    }
}
