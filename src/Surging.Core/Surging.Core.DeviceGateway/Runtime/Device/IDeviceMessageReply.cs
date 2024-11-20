using Surging.Core.DeviceGateway.Runtime.Core.Implementation;
using Surging.Core.DeviceGateway.Runtime.Device.Message;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Device
{
    public interface IDeviceMessageReply: IDeviceMessage,IMessage
    {
      

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        string Code { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        string Message { get; set; }

        bool IsSuccess { get; set; }

         IDeviceMessageReply Success(bool isSuccess);

         IDeviceMessageReply Failure(Exception e);

         IDeviceMessageReply Failure(StatusCode errorCode);

         IDeviceMessageReply Failure(string errorCode, string msg);

    }
}
