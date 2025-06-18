using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Module;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Protocol
{
    public abstract class ProtocolSupportProvider : IProtocolSupportProvider
    {       
        public abstract IObservable<IProtocolSupport> Create(ProtocolContext context);
         
        public virtual IObservable<IProtocolSupport> GetProtocolSupport(string Id)
        {  
            return Observable.Return(default(IProtocolSupport));
        }

        public virtual void Initialize()
        {
            
        }
    }
}
