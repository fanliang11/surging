using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Surging.Core.DeviceGateway.Runtime.Device.Message.Function
{
    internal class FunctionInvokeMessage : RespondDeviceMessage<IDeviceMessageReply>
    {
        public override MessageType MessageType { get; set; } = MessageType.INVOKE_FUNCTION;

        private List<FunctionParameter> Inputs { get; set; } = new List<FunctionParameter>();
        public string FunctionId { get; set; }

        
        public FunctionInvokeMessage AddInput(FunctionParameter parameter)
        {
            Inputs.Add(parameter);
            return this;
        }
         
        public FunctionInvokeMessage AddInput(string name, object value)
        {
            Inputs.Add(new FunctionParameter { Name = name,  Value=value }) ;
            return this;
        }

        public FunctionInvokeMessage AddInputs(Dictionary<string, object> parameters)
        {
            foreach (var param in parameters)
            {
                Inputs.Add(new FunctionParameter { Name = param.Key, Value = param.Value });
            }
            return this;
        }

        public void FromJson(JsonObject jsonObject)
        {
            Dictionary<string, object> inputParams = null;
            if (jsonObject.TryGetPropertyValue("functionId", out JsonNode? functionId))
                FunctionId = functionId.GetValue<string>();

            if (jsonObject.TryGetPropertyValue("inputs", out JsonNode? inputs))
                inputParams = inputs.GetValue<Dictionary<string, object>>();
            if (inputParams != null)
            {
                base.FromJson(new JsonObject(
                    jsonObject.Where(p => !p.Key.Equals("inputs")).ToList()
                    ));
                AddInputs(inputParams);

                return;
            }
            base.FromJson(jsonObject);
        }


        public override IDeviceMessageReply NewReply()
        {
            throw new NotImplementedException();
        }
    }
}
