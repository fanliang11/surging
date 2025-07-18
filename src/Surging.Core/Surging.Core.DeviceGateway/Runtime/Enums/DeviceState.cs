using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Enums
{
    public enum DeviceState: sbyte
    {
        //未知
          Unknown = 0,

        //在线
          Online = 1,

        //未激活
          NoActive = -3,

        //离线
          Offline = -1,

        //检查状态超时
          Timeout = -2
    }
}
