using System;

namespace Surging.Core.CPlatform.Runtime.Server.Implementation
{
    public class ServiceTokenGenerator : IServiceTokenGenerator
    {
        public string _serviceToken;
        public string GeneratorToken(string code)
        {
            bool enableToken;
            if (!bool.TryParse(code, out enableToken))
            {
                _serviceToken = code;
            }
            else
            {
                if (enableToken) _serviceToken = Guid.NewGuid().ToString("N");
                else _serviceToken = null;
            }
            return _serviceToken;
        }

        public string GetToken()
        {
            return _serviceToken;
        }
    }
}
