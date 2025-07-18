using Surging.Core.ApiGateWay;
using Surging.Core.ApiGateWay.OAuth;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.Protocol.WebService.Runtime;
using Surging.IModuleServices.Common;
using Surging.IModuleServices.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Modules.Common.Domain
{
    public class WebServiceService : WebServiceBehavior, IWebServiceService
    {
        private readonly IAuthorizationServerProvider _authorizationServerProvider;
        public WebServiceService()
        {
            _authorizationServerProvider = ServiceLocator.GetService<IAuthorizationServerProvider>();
        }
        public async Task<string> SayHello(string name)
        {
            var token = this.HeaderValue.Token;
            if (await _authorizationServerProvider.ValidateClientAuthentication(token))
                return $"Hello,{name}";
            else
                return " Please leave, stranger";
        }

        public async Task<string> Authentication(AuthenticationRequestData requestData)
        {
            var param = new Dictionary<string, object>();
            param.Add("requestData", requestData);
            AppConfig.AuthorizationRoutePath = "api/user/authentication";
            AppConfig.AuthorizationServiceKey = "User";
            AppConfig.AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(30);
            var result = await _authorizationServerProvider.GenerateTokenCredential(param);
            return result;
        }
    }
}
