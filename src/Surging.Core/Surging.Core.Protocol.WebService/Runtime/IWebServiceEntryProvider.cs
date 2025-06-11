using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.WebService.Runtime
{
    public interface IWebServiceEntryProvider
    {
        IEnumerable<WebServiceEntry> GetEntries();

        List<WebServiceEntry> CreateServiceEntries(Type service);
    }
}
