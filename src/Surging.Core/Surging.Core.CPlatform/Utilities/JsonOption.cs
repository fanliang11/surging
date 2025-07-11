using Surging.Core.KestrelHttpServer.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Utilities
{
    public  class JsonOption
    {
        public static JsonSerializerOptions SerializeOptions { get; set; } = new JsonSerializerOptions()
        { 
            WriteIndented = true,
            Converters = { new JsonElementJsonConverter() }

        };
    }
}
