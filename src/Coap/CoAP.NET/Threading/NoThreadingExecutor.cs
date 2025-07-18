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

namespace CoAP.Threading
{
    /// <summary>
    /// This <see cref="IExecutor"/> will execute tasks immediately in the calling thread.
    /// No threading will be used.
    /// </summary>
    public sealed class NoThreadingExecutor : IExecutor
    {
        internal NoThreadingExecutor()
        { }

        /// <inheritdoc/>
        public void Start(Action task)
        {
            task();
        }

        /// <inheritdoc/>
        public void Start(Action<Object> task, Object obj)
        {
            task(obj);
        }
    }
}
