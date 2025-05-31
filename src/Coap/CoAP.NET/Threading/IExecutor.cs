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
    /// Provides methods to execute tasks.
    /// </summary>
    public interface IExecutor
    {
        /// <summary>
        /// Starts a task without parameter.
        /// </summary>
        /// <param name="task">the task to run</param>
        void Start(Action task);
        /// <summary>
        /// Starts a task with a parameter.
        /// </summary>
        /// <param name="task">the task to run</param>
        /// <param name="obj">the parameter to be passed into the task</param>
        void Start(Action<Object> task, Object obj);
    }

    /// <summary>
    /// Executors.
    /// </summary>
    public static partial class Executors
    {
        /// <summary>
        /// This <see cref="IExecutor"/> will execute tasks immediately in the calling thread.
        /// </summary>
        public static readonly IExecutor NoThreading = new NoThreadingExecutor();
    }
}
