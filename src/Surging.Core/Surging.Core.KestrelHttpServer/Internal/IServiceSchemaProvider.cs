using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.KestrelHttpServer.Internal
{
    public interface IServiceSchemaProvider
    {
        IEnumerable<string> GetSchemaFilesPath();
    }
}
