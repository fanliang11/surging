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

namespace CoAP.Threading
{
    static partial class Executors
    {
        /// <summary>
        /// The default <see cref="IExecutor"/> based on <see cref="System.Threading.Tasks.Task"/>.
        /// </summary>
        public static readonly IExecutor Default = new TaskExecutor();

        /// <summary>
        /// The <see cref="IExecutor"/> based on <see cref="System.Threading.ThreadPool"/>.
        /// </summary>
        public static readonly IExecutor ThreadPool = new ThreadPoolExecutor();
    }
}
