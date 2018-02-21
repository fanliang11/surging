using Surging.Core.CPlatform.Cache;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Caching.AddressResolvers
{
    public interface IAddressResolver
    {
        ValueTask<CacheEndpoint> Resolver(string CacheId, CacheTargetType hashCode);
    }
}
