using Surging.Tools.Cli.Internal.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Tools.Cli.Internal
{
    public interface ITransportMessageEncoder
    {
        byte[] Encode(TransportMessage message);
    }
}
