using CoAP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Coap.Configurations
{
    internal class CoapOptions
    {
        public int Port {  get; set; }
        public Int32 SecurePort { get; set; }= CoapConstants.DefaultSecurePort;
        public Int32 HttpPort { get; set; } = 8080;
        public Int32 AckTimeout { get; set; } = CoapConstants.AckTimeout;
        public Double AckRandomFactor { get; set; } = CoapConstants.AckRandomFactor;
        public Double AckTimeoutScale { get; set; } = 2D;
        public Int32 MaxRetransmit { get; set; } = CoapConstants.MaxRetransmit;
        public Int32 MaxMessageSize { get; set; } = 1024;
        public Int32 DefaultBlockSize { get; set; } = CoapConstants.DefaultBlockSize;
        public Int32 BlockwiseStatusLifetime { get; set; } = 10 * 60 * 1000; // ms
        public Boolean UseRandomIDStart { get; set; } = true;
        public Boolean UseRandomTokenStart { get; set; } = true;
        public Int32 CropRotationPeriod { get; set; } = 2000; // ms
        public Int32 ExchangeLifetime { get; set; } = 247 * 1000; // ms
        public Int64 MarkAndSweepInterval { get; set; } = 10 * 1000; // ms
        public Int64 NotificationMaxAge { get; set; } = 128 * 1000; // ms
        public Int64 NotificationCheckIntervalTime { get; set; } = 24 * 60 * 60 * 1000; // ms
        public Int32 NotificationCheckIntervalCount { get; set; } = 100; // ms
        public Int32 NotificationReregistrationBackoff { get; set; } = 2000; // ms
        public Int32 ChannelReceiveBufferSize { get; set; }
        public Int32 ChannelSendBufferSize { get; set; }
        public Int32 ChannelReceivePacketSize { get; set; } = 2048;
    }
}
