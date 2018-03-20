using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Cache
{
   public  interface ICacheNodeProvider
    {
        IEnumerable<ServiceCache> GetServiceCaches();
    }
}
