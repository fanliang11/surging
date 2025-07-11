using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Swagger_V5.Internal
{
   public interface IServiceSchemaProvider
    {
        IEnumerable<string> GetSchemaFilesPath();
    }
}
