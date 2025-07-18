using Newtonsoft.Json;
using System.Collections.Generic;

namespace Surging.Core.Swagger_V5.Swagger.Model
{
    public abstract class SecurityScheme
    {
        public SecurityScheme()
        {
            Extensions = new Dictionary<string, object>();
        }

        public string Type { get; set; }

        public string Description { get; set; }

        [JsonExtensionData]
        public Dictionary<string, object> Extensions { get; private set; }
    }
}
