using Surging.Core.CPlatform.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Surging.Core.CPlatform.Diagnostics
{
    public static  class DiagnosticListenerExtensions
    {
        public const string Prefix = "Surging.Core.";
        public const string SurgingBeforeTransport= Prefix + nameof(WriteTransportBefore);
        public const string SurgingAfterTransport= Prefix + nameof(WriteTransportAfter);
        public const string SurgingErrorTransport = Prefix + nameof(WriteTransportError);

        public static void WriteTransportBefore(this DiagnosticListener diagnosticListener,TransportEventData eventData)
        {

            if (diagnosticListener.IsEnabled(SurgingBeforeTransport))
            {
                eventData.Headers = new TracingHeaders();
                diagnosticListener.Write(SurgingBeforeTransport, eventData);

            }
        }

        public static void WriteTransportAfter(this DiagnosticListener diagnosticListener, TransportEventData eventData)
        { 
            if (diagnosticListener.IsEnabled(SurgingAfterTransport))
            {
                eventData.Headers = new TracingHeaders();
                diagnosticListener.Write(SurgingAfterTransport, eventData);
            }
        }

        public static void WriteTransportError(this DiagnosticListener diagnosticListener, TransportErrorEventData eventData)
        {
            if (diagnosticListener.IsEnabled(SurgingErrorTransport))
            {
                eventData.Headers = new TracingHeaders();
                diagnosticListener.Write(SurgingErrorTransport, eventData);
            }
        }
    }
}
