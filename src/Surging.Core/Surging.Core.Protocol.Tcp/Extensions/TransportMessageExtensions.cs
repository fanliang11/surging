using Surging.Core.CPlatform.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Tcp.Extensions
{
    public static class TransportMessageExtensions
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsTcpDispatchMessage(this TransportMessage message)
        {
            return message.ContentType == typeof(byte[]).FullName;
        }
    }
}
