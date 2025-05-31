using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Grpc.Runtime
{
    public interface IGrpcServiceEntryProvider
    {
        List<GrpcServiceEntry> GetEntries();
    }
}
