/*
 * Copyright (c) 2011-2014, Longxiang He <helongxiang@smeshlink.com>,
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

namespace CoAP.Server.Resources
{
    /// <summary>
    /// Represents a periodic resource based on a timer.
    /// </summary>
    public class TimerResource : Resource
    {
        private Timer _timer;

        /// <summary>
        /// Instantiates.
        /// </summary>
        /// <param name="resourceIdentifier">the name</param>
        /// <param name="period">the period to notify</param>
        public TimerResource(String resourceIdentifier, Int32 period)
            : base(resourceIdentifier)
        { 
            Observable = true;
            _timer = new Timer(Tick, null, 0, period);
        }

        /// <summary>
        /// Fires a changed event.
        /// </summary>
        /// <param name="o"></param>
        protected virtual void Tick(Object o)
        {
            Changed();
        }
    }
}
