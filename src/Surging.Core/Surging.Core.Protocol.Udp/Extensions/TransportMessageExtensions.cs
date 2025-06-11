using Surging.Core.CPlatform.Messages;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Surging.Core.Protocol.Udp.Extensions
{
    public static class TransportMessageExtensions
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsUdpDispatchMessage(this TransportMessage message)
        {
            return message.ContentType == typeof(byte[]).FullName;
        }
    }
}
