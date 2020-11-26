using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Thrift.Runtime
{
    public interface IThriftServiceEntryProvider
    {
        List<ThriftServiceEntry> GetEntries();
    }
}
