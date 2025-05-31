using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Protocol
{
    public interface IProtocolSupports
    {
        public void Register(IProtocolSupport support);

        public void UnRegister(IProtocolSupport support);

        public void UnRegister(String id);

        IObservable<IProtocolSupport> GetProtocol(string protocol);

        ISubject<IProtocolSupport> GetProtocols();
    }
}
