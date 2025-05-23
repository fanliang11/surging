using Surging.Core.CPlatform.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Diagnostics
{
    public class DiagnosticMessage: TransportMessage
    {
        public string MessageName { get; set; }
    }
}
