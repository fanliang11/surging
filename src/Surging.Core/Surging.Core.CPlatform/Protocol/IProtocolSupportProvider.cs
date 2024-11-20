using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Protocol
{
    public  interface  IProtocolSupportProvider
    {
        void Initialize();

       IObservable<IProtocolSupport> GetProtocolSupport(string Id);
    }
}
