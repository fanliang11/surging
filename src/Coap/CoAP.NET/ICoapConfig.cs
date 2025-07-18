/*
 * Copyright (c) 2011-2015, Longxiang He <helongxiang@smeshlink.com>,
 * SmeshLink Technology Co.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY.
 * 
 * This file is part of the CoAP.NET, a CoAP framework in C#.
 * Please see README for more information.
 */

using System;

namespace CoAP
{
    /// <summary>
    /// Provides configuration for CoAP communication.
    /// </summary>
    public partial interface ICoapConfig : System.ComponentModel.INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the version of CoAP protocol.
        /// </summary>
        String Version { get; }
        /// <summary>
        /// Gets the default CoAP port for normal CoAP communication (not secure).
        /// </summary>
        Int32 DefaultPort { get; }
        /// <summary>
        /// Gets the default CoAP port for secure CoAP communication (coaps).
        /// </summary>
        Int32 DefaultSecurePort { get; }
        /// <summary>
        /// Gets the port which HTTP proxy is on.
        /// </summary>
        Int32 HttpPort { get; }

        Int32 AckTimeout { get; }
        Double AckRandomFactor { get; }
        Double AckTimeoutScale { get; }
        Int32 MaxRetransmit { get; }

        Int32 MaxMessageSize { get; }
        /// <summary>
        /// Gets the default preferred size of block in blockwise transfer.
        /// </summary>
        Int32 DefaultBlockSize { get; }
        Int32 BlockwiseStatusLifetime { get; }
        Boolean UseRandomIDStart { get; }
        Boolean UseRandomTokenStart { get; }
        
        Int64 NotificationMaxAge { get; }
        Int64 NotificationCheckIntervalTime { get; }
        Int32 NotificationCheckIntervalCount { get; }
        Int32 NotificationReregistrationBackoff { get; }

        String Deduplicator { get; }
        Int32 CropRotationPeriod { get; }
        Int32 ExchangeLifetime { get; }
        Int64 MarkAndSweepInterval { get; }

        Int32 ChannelReceiveBufferSize { get; }
        Int32 ChannelSendBufferSize { get; }
        Int32 ChannelReceivePacketSize { get; }

        /// <summary>
        /// Loads configuration from a config properties file.
        /// </summary>
        void Load(String configFile);

        /// <summary>
        /// Stores the configuration in a config properties file.
        /// </summary>
        void Store(String configFile);
    }
}
