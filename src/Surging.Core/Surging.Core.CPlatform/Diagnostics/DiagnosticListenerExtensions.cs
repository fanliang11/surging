using Surging.Core.CPlatform.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Surging.Core.CPlatform.Diagnostics
{
    public static class DiagnosticListenerExtensions
    {
        public const string DiagnosticListenerName = "SurgingDiagnosticListener";
        public const string Prefix = "Surging.Core.";
        public const string SurgingBeforeTransport = Prefix +".{0}."+ nameof(WriteTransportBefore);
        public const string SurgingAfterTransport= Prefix + ".{0}." + nameof(WriteTransportAfter);
        public const string SurgingErrorTransport = Prefix + ".{0}." + nameof(WriteTransportError);

        public static void WriteTransportBefore(this DiagnosticListener diagnosticListener,TransportType transportType, TransportEventData eventData)
        {

            if (diagnosticListener.IsEnabled(SurgingBeforeTransport))
            {
                eventData.Headers = new TracingHeaders();
                diagnosticListener.Write(string.Format(SurgingBeforeTransport,transportType), eventData);

            }
        }

        public static void WriteTransportAfter(this DiagnosticListener diagnosticListener, TransportType transportType, ReceiveEventData eventData)
        { 
            if (diagnosticListener.IsEnabled(SurgingAfterTransport))
            {
                eventData.Headers = new TracingHeaders();
                diagnosticListener.Write(string.Format(SurgingAfterTransport, transportType), eventData);
            }
        }

        public static void WriteTransportError(this DiagnosticListener diagnosticListener, TransportType transportType, TransportErrorEventData eventData)
        {
            if (diagnosticListener.IsEnabled(SurgingErrorTransport))
            {
                eventData.Headers = new TracingHeaders();
                diagnosticListener.Write(string.Format(SurgingErrorTransport, transportType), eventData);
            }
        }
    }
}
