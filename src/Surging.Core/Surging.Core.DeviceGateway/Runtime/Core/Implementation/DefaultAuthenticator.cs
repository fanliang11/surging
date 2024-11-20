
using Jint.Parser.Ast;
using Microsoft.Win32;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.DeviceGateway.Runtime.Device;
using Surging.Core.DeviceGateway.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core.Implementation
{
    public class DefaultAuthenticator : IAuthenticator
    {
        public IObservable<AuthenticationResult> Authenticate(IAuthenticationRequest request, IDeviceOperator deviceOperation)
        {
            var result = Observable.Return<AuthenticationResult>(default);
            if (request is DefaultAuthRequest)
            {
                var authRequest = request as DefaultAuthRequest;  
                var username = authRequest.UserName; 
                var password = authRequest.Password;
                String[] arr = username.Split("&");
                if (arr.Length <= 1)
                {
                    return Observable.Return(AuthenticationResult.Failure(StatusCode.CUSTOM_ERROR, "用户名格式错误"));
                }
                var requestSecureId = arr[0];
                long.TryParse(arr[1], out long time);
                if (Math.Abs(Utility.CurrentTimeMillis() - time) > TimeSpan.FromMinutes(10).TotalMilliseconds)
                {
                    return Observable.Return(AuthenticationResult.Failure(StatusCode.CUSTOM_ERROR, "时间不一致"));
                }
                var configs = deviceOperation.GetConfigs("secureId", "secureKey").Subscribe(p =>
                {
                    try
                    {
                        var secureId = p.GetValue("secureId").Convert<string>();
                        var secureKey = p.GetValue("secureKey").Convert<string>();
                        var encryptStr = $"{username}&{secureKey}".GetMd5Hash();
                        if (requestSecureId.Equals(secureId) && encryptStr.Equals(password))
                        {
                            result= result.Publish(AuthenticationResult.Success(deviceOperation.GetDeviceId()));
                        }
                        else
                        {
                            result= result.Publish(AuthenticationResult.Failure(StatusCode.CUSTOM_ERROR, "验证失败,密钥错误"));

                        }
                    }
                    catch (Exception ex)
                    {
                        result = result.Publish(AuthenticationResult.Failure(StatusCode.CUSTOM_ERROR, "请求参数格式错误"));
                    }
                });
            }
            else
            result = Observable.Return<AuthenticationResult>(AuthenticationResult.Failure(StatusCode.CUSTOM_ERROR, "不支持请求参数类型"));
            return result;
        }

        public IObservable<AuthenticationResult> Authenticate(IAuthenticationRequest request, IDeviceRegistry registry)
        {
            var result = Observable.Return<AuthenticationResult>(default);
            var authRequest = request as DefaultAuthRequest;
            registry
              .GetDevice(authRequest.DeviceId)
              .Subscribe( p =>Authenticate(request, p).Subscribe(authResult => result = result.Publish(authResult)));
            return result;
        }
    }
}
