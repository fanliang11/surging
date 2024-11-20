using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Protocol
{
    internal class DefaultProtocolSupportProvider : ProtocolSupportProvider
    {
        private readonly IProtocolSupports _protocolSupport;
        private readonly ProtocolContext _context;
        private readonly  Type[] _types;
        private readonly ILogger<DefaultProtocolSupportProvider> _logger;
        public DefaultProtocolSupportProvider(IProtocolSupports protocolSupport,  ProtocolContext context, ILogger<DefaultProtocolSupportProvider> logger,
           Type[] types){
            _protocolSupport = protocolSupport;
           _context = context;
            _types = types;
            _logger = logger;

        }

        public override void Initialize()
        { 
            _types.Where(p => p != this.GetType() && typeof(ProtocolSupportProvider).GetTypeInfo().IsAssignableFrom(p) &&  !p.IsAbstract).ToList().ForEach(protocolType =>
            {
                var supportProvider = (ProtocolSupportProvider)Activator.CreateInstance(protocolType);

                supportProvider.Create(_context).Subscribe(p =>
                {
                    if (p != null)
                    {
                        _protocolSupport.Register(p);
                    }
                });
               
            });
           
        }

        public override IObservable<IProtocolSupport> GetProtocolSupport(string Id)
        {
            return _protocolSupport.GetProtocol(Id);
        }

        public override IObservable<IProtocolSupport> Create(ProtocolContext context)
        { 
            return Observable.Return(default(IProtocolSupport));
        }
    }
}
