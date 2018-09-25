using System.Collections.Generic;

namespace Surging.Core.Swagger
{
    public class OAuth2Scheme : SecurityScheme
    {
        public OAuth2Scheme()
        {
            Type = "oauth2";
        }

        public string Flow { get; set; }

        public string AuthorizationUrl { get; set; }

        public string TokenUrl { get; set; }

        public IDictionary<string, string> Scopes { get; set; }
    }
}
