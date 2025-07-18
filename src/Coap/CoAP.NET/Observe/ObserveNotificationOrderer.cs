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
using System.Threading;
using CoAP.Util;

namespace CoAP.Observe
{
    /// <summary>
    /// This class holds the state of an observe relation such
    /// as the timeout of the last notification and the current number.
    /// </summary>
    public class ObserveNotificationOrderer
    {
        readonly ICoapConfig _config;
        private Int32 _number;
        private DateTime _timestamp;

        public ObserveNotificationOrderer()
            : this(null)
        { }

        public ObserveNotificationOrderer(ICoapConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Gets a new observe option number.
        /// </summary>
        /// <returns>a new observe option number</returns>
        public Int32 GetNextObserveNumber()
        {
            Int32 next = Interlocked.Increment(ref _number);
            while (next >= 1 << 24)
            {
                Interlocked.CompareExchange(ref _number, 0, next);
                next = Interlocked.Increment(ref _number);
            }
            return next;
        }

        /// <summary>
        /// Gets the current notification number.
        /// </summary>
        public Int32 Current
        {
            get { return _number; }
        }

        public DateTime Timestamp
        {
            get { return _timestamp; }
            set { _timestamp = value; }
        }

        public Boolean IsNew(Response response)
        {
            Int32? obs = response.Observe;
            if (!obs.HasValue)
            {
                // this is a final response, e.g., error or proactive cancellation
                return true;
            }

            // Multiple responses with different notification numbers might
            // arrive and be processed by different threads. We have to
            // ensure that only the most fresh one is being delivered.
            // We use the notation from the observe draft-08.
            DateTime T1 = Timestamp;
            DateTime T2 = DateTime.Now;
            Int32 V1 = Current;
            Int32 V2 = obs.Value;
            Int64 notifMaxAge = (_config ?? CoapConfig.Default).NotificationMaxAge;
            if (V1 < V2 && V2 - V1 < 1 << 23
                    || V1 > V2 && V1 - V2 > 1 << 23
                    || T2 > T1.AddMilliseconds(notifMaxAge))
            {
                Timestamp = T2;
                _number = V2;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
