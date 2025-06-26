using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Protocol
{
    public class ProtocolSupports : IProtocolSupports
    {
        private ConcurrentDictionary<string,IProtocolSupport> _protocolSupports = new ConcurrentDictionary<string, IProtocolSupport>();
        public IObservable<IProtocolSupport> GetProtocol(string protocol)
        {
            _protocolSupports.TryGetValue(protocol, out IProtocolSupport protocolSupport);
             return Observable.Return(protocolSupport);
        }

        public ISubject<IProtocolSupport> GetProtocols()
        {
            var subject = new AsyncSubject<IProtocolSupport>();
            var protocolSupports=  _protocolSupports.Values.ToList();
            protocolSupports.ForEach(p=>subject.OnNext(p));
            return subject;
        }

        public void Register(IProtocolSupport support)
        {
            _protocolSupports.AddOrUpdate(support.Id, support, (key, value) => support);
        }

        public void UnRegister(IProtocolSupport support)
        {
            _protocolSupports.TryRemove(support.Id,out IProtocolSupport protocolSupport);
        }

        public void UnRegister(string id)
        {
            _protocolSupports.TryRemove(id, out IProtocolSupport protocolSupport);
        }
    }
}
