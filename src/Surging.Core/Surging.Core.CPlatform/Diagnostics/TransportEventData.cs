using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Diagnostics
{
    public class TransportEventData : EventData
    {
        public TransportEventData(DiagnosticMessage message,string address)
            : base(Guid.Parse(message.Id))
        {
            Message = message;
            BrokerAddress = address;
        }

        public string BrokerAddress { get; set; }

        public TracingHeaders Headers { get; set; }

        public DiagnosticMessage Message { get; set; }

    }
} 
