using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Device.Message.Function
{
    internal class FunctionInvokeMessageReply : CommonDeviceMessageReply<FunctionInvokeMessageReply>
    {
        public override MessageType MessageType { get; set; } = MessageType.INVOKE_FUNCTION_REPLY;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        private string FunctionId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        private object Output { get; set; }

        public void FromJson(JsonObject jsonObject)
        {
           base.FromJson(jsonObject);
            this.FunctionId = jsonObject["functionId"].GetValue<string>();
            this.Output = jsonObject["output"].GetValue<object>();
        }


        public FunctionInvokeMessageReply Success(Object output)
        {
            this.Output = output;
            IsSuccess = true;
            return this;
                    
        }

        public   FunctionInvokeMessageReply Success(string deviceId,
                                                         string functionId,
                                                         string messageId,
                                                         object output)
        { 
            FunctionId=functionId;
            Output=output;
            IsSuccess=true;
            DeviceId=deviceId;
            MessageId=messageId;
            return this;
        }

        public  FunctionInvokeMessageReply Failure(string deviceId,
                                                         string functionId,
                                                         string messageId,
                                                         string message)
        {
            FunctionId = functionId;
            Message = message;
            IsSuccess = true;
            DeviceId = deviceId;
            MessageId = messageId;

            return this;
        }
    }
}
