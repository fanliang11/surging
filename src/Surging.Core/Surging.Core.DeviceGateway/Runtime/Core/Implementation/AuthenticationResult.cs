using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core.Implementation
{
    public class AuthenticationResult
    {

        public AuthenticationResult() { }

        public AuthenticationResult(string deviceId,string s)
        {

            if (!string.IsNullOrEmpty(s))
            {
                Msg = s;
                Code = StatusCode.SUCCESS;
            }
            else
            {
                Msg = "授权通过";
                Code = StatusCode.SUCCESS;
                DeviceId = deviceId;
            }

            IsSuccess= Code == StatusCode.SUCCESS;
        }
        public AuthenticationResult(StatusCode code, string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                Msg = s;
                Code = code;
            }
            IsSuccess = code == StatusCode.SUCCESS;
        }
        /// <summary>
        /// 响应编码
        /// </summary>
        [DataMember]
        public bool IsSuccess { get; set; } = false;


        /// <summary>
        /// 响应编码
        /// </summary>
        [DataMember]
        public StatusCode Code { get; set; } = StatusCode.SUCCESS;

        [DataMember]
        public string DeviceId { get; set; }
        /// <summary>
        /// 返回信息
        /// </summary>
        [DataMember]
        public string Msg { get; set; }

        public static AuthenticationResult Failure(StatusCode c, string s) => new AuthenticationResult(c, s);
        public static AuthenticationResult Success(string device,string s) => new AuthenticationResult(device,s);

        public static AuthenticationResult Success(string device) => new AuthenticationResult(device, null);

    }
}
