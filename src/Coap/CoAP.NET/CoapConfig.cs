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
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;

namespace CoAP
{
    /// <summary>
    /// Default implementation of <see cref="ICoapConfig"/>.
    /// </summary>
    public partial class CoapConfig : ICoapConfig
    {
        private static ICoapConfig _default;

        public static ICoapConfig Default
        {
            get
            {
                if (_default == null)
                {
                    lock (typeof(CoapConfig))
                    {
                        if (_default == null)
                            _default = LoadConfig();
                    }
                }
                return _default;
            }
        }

        private Int32 _port;
        private Int32 _securePort = CoapConstants.DefaultSecurePort;
        private Int32 _httpPort = 8080;
        private Int32 _ackTimeout = CoapConstants.AckTimeout;
        private Double _ackRandomFactor = CoapConstants.AckRandomFactor;
        private Double _ackTimeoutScale = 2D;
        private Int32 _maxRetransmit = CoapConstants.MaxRetransmit;
        private Int32 _maxMessageSize = 1024;
        private Int32 _defaultBlockSize = CoapConstants.DefaultBlockSize;
        private Int32 _blockwiseStatusLifetime = 10 * 60 * 1000; // ms
        private Boolean _useRandomIDStart = true;
        private Boolean _useRandomTokenStart = true;
        private String _deduplicator = CoAP.Deduplication.DeduplicatorFactory.MarkAndSweepDeduplicator;
        private Int32 _cropRotationPeriod = 2000; // ms
        private Int32 _exchangeLifetime = 247 * 1000; // ms
        private Int64 _markAndSweepInterval = 10 * 1000; // ms
        private Int64 _notificationMaxAge = 128 * 1000; // ms
        private Int64 _notificationCheckIntervalTime = 24 * 60 * 60 * 1000; // ms
        private Int32 _notificationCheckIntervalCount = 100; // ms
        private Int32 _notificationReregistrationBackoff = 2000; // ms
        private Int32 _channelReceiveBufferSize;
        private Int32 _channelSendBufferSize;
        private Int32 _channelReceivePacketSize = 2048;

        /// <summary>
        /// Instantiate.
        /// </summary>
        public CoapConfig()
        {
            _port = Spec.DefaultPort;
        }

        /// <inheritdoc/>
        public String Version
        {
            get { return Spec.Name; }
        }
        
        /// <inheritdoc/>
        public Int32 DefaultPort
        {
            get { return _port; }
            set
            {
                if (_port != value)
                {
                    _port = value;
                    NotifyPropertyChanged("DefaultPort");
                }
            }
        }

        /// <inheritdoc/>
        public Int32 DefaultSecurePort
        {
            get { return _securePort; }
            set
            {
                if (_securePort != value)
                {
                    _securePort = value;
                    NotifyPropertyChanged("DefaultSecurePort");
                }
            }
        }

        /// <inheritdoc/>
        public Int32 HttpPort
        {
            get { return _httpPort; }
            set
            {
                if (_httpPort != value)
                {
                    _httpPort = value;
                    NotifyPropertyChanged("HttpPort");
                }
            }
        }

        /// <inheritdoc/>
        public Int32 AckTimeout
        {
            get { return _ackTimeout; }
            set
            {
                if (_ackTimeout != value)
                {
                    _ackTimeout = value;
                    NotifyPropertyChanged("AckTimeout");
                }
            }
        }

        /// <inheritdoc/>
        public Double AckRandomFactor
        {
            get { return _ackRandomFactor; }
            set
            {
                if (_ackRandomFactor != value)
                {
                    _ackRandomFactor = value;
                    NotifyPropertyChanged("AckRandomFactor");
                }
            }
        }

        /// <inheritdoc/>
        public Double AckTimeoutScale
        {
            get { return _ackTimeoutScale; }
            set
            {
                if (_ackTimeoutScale != value)
                {
                    _ackTimeoutScale = value;
                    NotifyPropertyChanged("AckTimeoutScale");
                }
            }
        }

        /// <inheritdoc/>
        public Int32 MaxRetransmit
        {
            get { return _maxRetransmit; }
            set
            {
                if (_maxRetransmit != value)
                {
                    _maxRetransmit = value;
                    NotifyPropertyChanged("MaxRetransmit");
                }
            }
        }

        /// <inheritdoc/>
        public Int32 MaxMessageSize
        {
            get { return _maxMessageSize; }
            set
            {
                if (_maxMessageSize != value)
                {
                    _maxMessageSize = value;
                    NotifyPropertyChanged("MaxMessageSize");
                }
            }
        }

        /// <inheritdoc/>
        public Int32 DefaultBlockSize
        {
            get { return _defaultBlockSize; }
            set
            {
                if (_defaultBlockSize != value)
                {
                    _defaultBlockSize = value;
                    NotifyPropertyChanged("DefaultBlockSize");
                }
            }
        }

        /// <inheritdoc/>
        public Int32 BlockwiseStatusLifetime
        {
            get { return _blockwiseStatusLifetime; }
            set
            {
                if (_blockwiseStatusLifetime != value)
                {
                    _blockwiseStatusLifetime = value;
                    NotifyPropertyChanged("BlockwiseStatusLifetime");
                }
            }
        }

        /// <inheritdoc/>
        public Boolean UseRandomIDStart
        {
            get { return _useRandomIDStart; }
            set
            {
                if (_useRandomIDStart != value)
                {
                    _useRandomIDStart = value;
                    NotifyPropertyChanged("UseRandomIDStart");
                }
            }
        }

        /// <inheritdoc/>
        public Boolean UseRandomTokenStart
        {
            get { return _useRandomTokenStart; }
            set
            {
                if (_useRandomTokenStart != value)
                {
                    _useRandomTokenStart = value;
                    NotifyPropertyChanged("UseRandomTokenStart");
                }
            }
        }

        /// <inheritdoc/>
        public String Deduplicator
        {
            get { return _deduplicator; }
            set
            {
                if (_deduplicator != value)
                {
                    _deduplicator = value;
                    NotifyPropertyChanged("Deduplicator");
                }
            }
        }

        /// <inheritdoc/>
        public Int32 CropRotationPeriod
        {
            get { return _cropRotationPeriod; }
            set
            {
                if (_cropRotationPeriod != value)
                {
                    _cropRotationPeriod = value;
                    NotifyPropertyChanged("CropRotationPeriod");
                }
            }
        }

        /// <inheritdoc/>
        public Int32 ExchangeLifetime
        {
            get { return _exchangeLifetime; }
            set
            {
                if (_exchangeLifetime != value)
                {
                    _exchangeLifetime = value;
                    NotifyPropertyChanged("ExchangeLifetime");
                }
            }
        }

        /// <inheritdoc/>
        public Int64 MarkAndSweepInterval
        {
            get { return _markAndSweepInterval; }
            set
            {
                if (_markAndSweepInterval != value)
                {
                    _markAndSweepInterval = value;
                    NotifyPropertyChanged("MarkAndSweepInterval");
                }
            }
        }

        /// <inheritdoc/>
        public Int64 NotificationMaxAge
        {
            get { return _notificationMaxAge; }
            set
            {
                if (_notificationMaxAge != value)
                {
                    _notificationMaxAge = value;
                    NotifyPropertyChanged("NotificationMaxAge");
                }
            }
        }

        /// <inheritdoc/>
        public Int64 NotificationCheckIntervalTime
        {
            get { return _notificationCheckIntervalTime; }
            set
            {
                if (_notificationCheckIntervalTime != value)
                {
                    _notificationCheckIntervalTime = value;
                    NotifyPropertyChanged("NotificationCheckIntervalTime");
                }
            }
        }

        /// <inheritdoc/>
        public Int32 NotificationCheckIntervalCount
        {
            get { return _notificationCheckIntervalCount; }
            set
            {
                if (_notificationCheckIntervalCount != value)
                {
                    _notificationCheckIntervalCount = value;
                    NotifyPropertyChanged("NotificationCheckIntervalCount");
                }
            }
        }

        /// <inheritdoc/>
        public Int32 NotificationReregistrationBackoff
        {
            get { return _notificationReregistrationBackoff; }
            set
            {
                if (_notificationReregistrationBackoff != value)
                {
                    _notificationReregistrationBackoff = value;
                    NotifyPropertyChanged("NotificationReregistrationBackoff");
                }
            }
        }

        /// <inheritdoc/>
        public Int32 ChannelReceiveBufferSize
        {
            get { return _channelReceiveBufferSize; }
            set
            {
                if (_channelReceiveBufferSize != value)
                {
                    _channelReceiveBufferSize = value;
                    NotifyPropertyChanged("ChannelReceiveBufferSize");
                }
            }
        }

        /// <inheritdoc/>
        public Int32 ChannelSendBufferSize
        {
            get { return _channelSendBufferSize; }
            set
            {
                if (_channelSendBufferSize != value)
                {
                    _channelSendBufferSize = value;
                    NotifyPropertyChanged("ChannelSendBufferSize");
                }
            }
        }

        /// <inheritdoc/>
        public Int32 ChannelReceivePacketSize
        {
            get { return _channelReceivePacketSize; }
            set
            {
                if (_channelReceivePacketSize != value)
                {
                    _channelReceivePacketSize = value;
                    NotifyPropertyChanged("ChannelReceivePacketSize");
                }
            }
        }

        /// <inheritdoc/>
        public void Load(String configFile)
        {
            String[] lines = File.ReadAllLines(configFile);
            NameValueCollection nvc = new NameValueCollection(lines.Length, StringComparer.OrdinalIgnoreCase);
            foreach (String line in lines)
            {
                String[] tmp = line.Split(new Char[] { '=' }, 2);
                if (tmp.Length == 2)
                    nvc[tmp[0]] = tmp[1];
            }

            DefaultPort = GetInt32(nvc, "DefaultPort", "DEFAULT_COAP_PORT", DefaultPort);
            DefaultSecurePort = GetInt32(nvc, "DefaultSecurePort", "DEFAULT_COAPS_PORT", DefaultSecurePort);
            HttpPort = GetInt32(nvc, "HttpPort", "HTTP_PORT", HttpPort);
            AckTimeout = GetInt32(nvc, "AckTimeout", "ACK_TIMEOUT", AckTimeout);
            AckRandomFactor = GetDouble(nvc, "AckRandomFactor", "ACK_RANDOM_FACTOR", AckRandomFactor);
            AckTimeoutScale = GetDouble(nvc, "AckTimeoutScale", "ACK_TIMEOUT_SCALE", AckTimeoutScale);
            MaxRetransmit = GetInt32(nvc, "MaxRetransmit", "MAX_RETRANSMIT", MaxRetransmit);
            MaxMessageSize = GetInt32(nvc, "MaxMessageSize", "MAX_MESSAGE_SIZE", MaxMessageSize);
            DefaultBlockSize = GetInt32(nvc, "DefaultBlockSize", "DEFAULT_BLOCK_SIZE", DefaultBlockSize);
            UseRandomIDStart = GetBoolean(nvc, "UseRandomIDStart", "USE_RANDOM_MID_START", UseRandomIDStart);
            UseRandomTokenStart = GetBoolean(nvc, "UseRandomTokenStart", "USE_RANDOM_TOKEN_START", UseRandomTokenStart);
            Deduplicator = GetString(nvc, "Deduplicator", "DEDUPLICATOR", Deduplicator);
            CropRotationPeriod = GetInt32(nvc, "CropRotationPeriod", "CROP_ROTATION_PERIOD", CropRotationPeriod);
            ExchangeLifetime = GetInt32(nvc, "ExchangeLifetime", "EXCHANGE_LIFETIME", ExchangeLifetime);
            MarkAndSweepInterval = GetInt64(nvc, "MarkAndSweepInterval", "MARK_AND_SWEEP_INTERVAL", MarkAndSweepInterval);
            NotificationMaxAge = GetInt64(nvc, "NotificationMaxAge", "NOTIFICATION_MAX_AGE", NotificationMaxAge);
            NotificationCheckIntervalTime = GetInt64(nvc, "NotificationCheckIntervalTime", "NOTIFICATION_CHECK_INTERVAL", NotificationCheckIntervalTime);
            NotificationCheckIntervalCount = GetInt32(nvc, "NotificationCheckIntervalCount", "NOTIFICATION_CHECK_INTERVAL_COUNT", NotificationCheckIntervalCount);
            NotificationReregistrationBackoff = GetInt32(nvc, "NotificationReregistrationBackoff", "NOTIFICATION_REREGISTRATION_BACKOFF", NotificationReregistrationBackoff);
            ChannelReceiveBufferSize = GetInt32(nvc, "ChannelReceiveBufferSize", "UDP_CONNECTOR_RECEIVE_BUFFER", ChannelReceiveBufferSize);
            ChannelSendBufferSize = GetInt32(nvc, "ChannelSendBufferSize", "UDP_CONNECTOR_SEND_BUFFER", ChannelSendBufferSize);
            ChannelReceivePacketSize = GetInt32(nvc, "ChannelReceivePacketSize", "UDP_CONNECTOR_DATAGRAM_SIZE", ChannelReceivePacketSize);
        }

        /// <inheritdoc/>
        public void Store(String configFile)
        {
            using (StreamWriter w = new StreamWriter(configFile))
            {
                w.Write("DefaultPort="); w.WriteLine(DefaultPort);
                w.Write("DefaultSecurePort="); w.WriteLine(DefaultSecurePort);
                w.Write("HttpPort="); w.WriteLine(HttpPort);
                w.Write("AckTimeout="); w.WriteLine(AckTimeout);
                w.Write("AckRandomFactor="); w.WriteLine(AckRandomFactor);
                w.Write("AckTimeoutScale="); w.WriteLine(AckTimeoutScale);
                w.Write("MaxRetransmit="); w.WriteLine(MaxRetransmit);
                w.Write("MaxMessageSize="); w.WriteLine(MaxMessageSize);
                w.Write("DefaultBlockSize="); w.WriteLine(DefaultBlockSize);
                w.Write("UseRandomIDStart="); w.WriteLine(UseRandomIDStart);
                w.Write("UseRandomTokenStart="); w.WriteLine(UseRandomTokenStart);
                w.Write("Deduplicator="); w.WriteLine(Deduplicator);
                w.Write("CropRotationPeriod="); w.WriteLine(CropRotationPeriod);
                w.Write("ExchangeLifetime="); w.WriteLine(ExchangeLifetime);
                w.Write("MarkAndSweepInterval="); w.WriteLine(MarkAndSweepInterval);
                w.Write("NotificationMaxAge="); w.WriteLine(NotificationMaxAge);
                w.Write("NotificationCheckIntervalTime="); w.WriteLine(NotificationCheckIntervalTime);
                w.Write("NotificationCheckIntervalCount="); w.WriteLine(NotificationCheckIntervalCount);
                w.Write("NotificationReregistrationBackoff="); w.WriteLine(NotificationReregistrationBackoff);
                w.Write("ChannelReceiveBufferSize="); w.WriteLine(ChannelReceiveBufferSize);
                w.Write("ChannelSendBufferSize="); w.WriteLine(ChannelSendBufferSize);
                w.Write("ChannelReceivePacketSize="); w.WriteLine(ChannelReceivePacketSize);
            }
        }

        private static String GetString(NameValueCollection nvc, String key1, String key2, String defaultValue)
        {
            return nvc[key1] ?? nvc[key2] ?? defaultValue;
        }

        private static Int32 GetInt32(NameValueCollection nvc, String key1, String key2, Int32 defaultValue)
        {
            String value = GetString(nvc, key1, key2, null);
            Int32 result;
            return !String.IsNullOrEmpty(value) && Int32.TryParse(value, out result) ? result : defaultValue;
        }

        private static Int64 GetInt64(NameValueCollection nvc, String key1, String key2, Int64 defaultValue)
        {
            String value = GetString(nvc, key1, key2, null);
            Int64 result;
            return !String.IsNullOrEmpty(value) && Int64.TryParse(value, out result) ? result : defaultValue;
        }

        private static Double GetDouble(NameValueCollection nvc, String key1, String key2, Double defaultValue)
        {
            String value = GetString(nvc, key1, key2, null);
            Double result;
            return !String.IsNullOrEmpty(value) && Double.TryParse(value, out result) ? result : defaultValue;
        }

        private static Boolean GetBoolean(NameValueCollection nvc, String key1, String key2, Boolean defaultValue)
        {
            String value = GetString(nvc, key1, key2, null);
            Boolean result;
            return !String.IsNullOrEmpty(value) && Boolean.TryParse(value, out result) ? result : defaultValue;
        }

        private static ICoapConfig LoadConfig()
        {
            // TODO may have configuration file here
            return new CoapConfig();
        }
        
        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
