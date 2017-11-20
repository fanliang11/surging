using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.ApiGateWay.OAuth
{
    public interface IAuthorizationServerProvider
    {
        Task<string> GenerateTokenCredential(Dictionary<string, object> parameters);

        Task<bool> ValidateClientAuthentication(string token);

        string GetPayloadString(string token);
    }
}
