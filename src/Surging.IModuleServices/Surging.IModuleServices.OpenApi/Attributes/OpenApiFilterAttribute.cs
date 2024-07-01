using Surging.Core.ApiGateWay;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.KestrelHttpServer.Filters;
using Surging.Core.KestrelHttpServer.Filters.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Surging.IModuleServices.OpenApi.Attributes
{
    internal class OpenApiFilterAttribute : IActionFilter
    {
        private readonly IServiceEntryLocate _serviceEntryLocate; 
        public OpenApiFilterAttribute()
        {
            _serviceEntryLocate = ServiceLocator.GetService<IServiceEntryLocate>(); 
        }

        public Task OnActionExecuted(ActionExecutedContext filterContext)
        {
            return Task.CompletedTask;
        }

        public async Task OnActionExecuting(ActionExecutingContext filterContext)
        {
            var serviceEntry = _serviceEntryLocate.Locate(filterContext.Message);

            var result = HttpResultMessage<object>.Create(true, null);
            if (serviceEntry == null) return;
            else
            {
                if (serviceEntry.Attributes.Any(p => p.GetType() == typeof(OpenApiAttribute)))
                {
                    var appId = filterContext.Message.Parameters["AppId"]?.ToString();
                    var stamp = filterContext.Message.Parameters["TimeStamp"]?.ToString();
                    var random = filterContext.Message.Parameters["Random"]?.ToString();

                    var sign = filterContext.Message.Parameters["Sign"]?.ToString();
                    if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(stamp) || string.IsNullOrEmpty(sign))
                    {
                        filterContext.Result = new HttpResultMessage<object> { IsSucceed = false, StatusCode = (int)ServiceStatusCode.RequestError, Message = "Request error" };
                        return;
                    }
                    else if (long.TryParse(stamp, out long timeStamp))
                    {
                        var time = DateTimeConverter.UnixTimestampToDateTime(timeStamp);
                        var seconds = (DateTime.UtcNow - time).TotalSeconds;
                        if (seconds <= 3560 && seconds >= 0)
                        { 
                            var appSecret = "appSecret_test";
                            if (appId != "app_12")
                            {
                                filterContext.Result = new HttpResultMessage<object> { IsSucceed = false, StatusCode = (int)ServiceStatusCode.RequestError, Message = " Appid  not exist" };
                            }
                            else if (SHA256HexHashString($"{appId}{appSecret}{random}{stamp}", true) != sign)
                            {
                                filterContext.Result = new HttpResultMessage<object> { IsSucceed = false, StatusCode = (int)ServiceStatusCode.AuthorizationFailed, Message = "Invalid authentication credentials" };

                            }
                        }
                        else
                        {
                            filterContext.Result = new HttpResultMessage<object> { IsSucceed = false, StatusCode = (int)ServiceStatusCode.AuthorizationFailed, Message = "Time out" };

                        }
                    }
                    else
                    {
                        filterContext.Result = new HttpResultMessage<object> { IsSucceed = false, StatusCode = (int)ServiceStatusCode.RequestError, Message = "timestamp should be of long type" };

                    }

                }
            }
        }



        private string SHA256HexHashString(string stringIn, bool isLowerCase)
        {
            string hashString;
            using (var hmacsha256 = SHA256.Create())
            {
                var hash = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(stringIn));
                hashString = BitConverter.ToString(hash).Replace("-", string.Empty);
                return isLowerCase ? hashString.ToLower() : hashString.ToUpper();
            }
            return hashString;
        }

        private string ToHex(byte[] bytes)
        {
            StringBuilder result = new StringBuilder(bytes.Length * 2);
            for (int i = 0; i < bytes.Length; i++)
                result.Append(Convert.ToString(bytes[i], 16));
            return result.ToString();
        }
    }
}
