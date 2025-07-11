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
using System.Threading.Tasks;

namespace CoAP.Threading
{
    /// <summary>
    /// <see cref="IExecutor"/> that use the <see cref="Task"/>.
    /// </summary>
    public sealed class TaskExecutor : IExecutor
    {
        /// <inheritdoc/>
        public void Start(Action task)
        {
            Task.Factory.StartNew(task);
        }

        /// <inheritdoc/>
        public void Start(Action<Object> task, Object obj)
        {
            Task.Factory.StartNew(task, obj);
        }
    }
}
