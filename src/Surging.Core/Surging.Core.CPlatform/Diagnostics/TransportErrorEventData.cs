using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Diagnostics
{
   public class TransportErrorEventData : EventData
    {
        public TransportErrorEventData(DiagnosticMessage message,Exception ex)
          : base(Guid.Parse(message.Id))
        {
            Message = message;
            Exception = ex;
        }


        public Exception Exception { get; set; }

        public TracingHeaders Headers { get; set; }

        public DiagnosticMessage Message { get; set; }
    }
}
