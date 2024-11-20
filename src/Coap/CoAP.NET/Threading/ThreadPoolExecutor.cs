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

namespace CoAP.Threading
{
    /// <summary>
    /// <see cref="IExecutor"/> that use the <see cref="ThreadPool"/>.
    /// </summary>
    public sealed class ThreadPoolExecutor : IExecutor
    {
        /// <inheritdoc/>
        public void Start(Action task)
        {
            ThreadPool.QueueUserWorkItem(o => task());
        }

        /// <inheritdoc/>
        public void Start(Action<Object> task, Object obj)
        {
            ThreadPool.QueueUserWorkItem(o => task(o), obj);
        }
    }
}
