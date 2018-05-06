using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.ProxyGenerator.Interceptors
{
    public interface ICacheInvocation : IInvocation
    {
        string[] CacheKey { get; }

        List<Attribute> Attributes { get; }
    }
}
