using Surging.Core.CPlatform.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Ioc
{
    public delegate Task ServerReceivedDelegate(TransportMessage message);
    public interface IServiceBehavior
    {
        public string MessageId { get; }
        event ServerReceivedDelegate Received;
    }
}
