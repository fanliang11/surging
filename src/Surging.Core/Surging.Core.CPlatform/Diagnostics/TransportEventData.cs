using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Diagnostics
{
    public class TransportEventData : EventData
    {
        public TransportEventData(DiagnosticMessage message, string  method, string traceId, string address)
            : base(Guid.Parse(message.Id))
        {
            Message = message;
            RemoteAddress = address;
            Method = method;
            TraceId = traceId;
        }

        public string TraceId { get; set; }

        public string Method { get; set; }

        public string RemoteAddress { get; set; }

        public TracingHeaders Headers { get; set; }

        public DiagnosticMessage Message { get; set; }

    }
} 
