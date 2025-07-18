using Surging.Core.DeviceGateway.Runtime.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Device
{
    public class DeviceResponse
    { 
            public DeviceResponse() { }

            public DeviceResponse(string s, string deviceId)
            {
                DeviceId = deviceId;
                if (!string.IsNullOrEmpty(s))
                {
                    Msg = s;
                    IsSuccess = true;
                    Code = EnumReturnCode.SUCCESS;
                }
            }
            public DeviceResponse(EnumReturnCode code, string s,string deviceId)
            {
                DeviceId = deviceId;
                if (!string.IsNullOrEmpty(s))
                {
                    Msg = s;
                    Code = code;
                    IsSuccess= code== EnumReturnCode.SUCCESS; 
                }
            }
         
            public EnumReturnCode Code { get; set; } = EnumReturnCode.SUCCESS;

            public string DeviceId { get; set; }


            public bool IsSuccess { get; set; } = true;

            public string Msg { get; set; }

            public static DeviceResponse Failure(EnumReturnCode c, string s, string deviceId) => new DeviceResponse(c, s, deviceId);
            public static DeviceResponse Successed(string s,string deviceId) => new DeviceResponse(s, deviceId);
        }
    } 