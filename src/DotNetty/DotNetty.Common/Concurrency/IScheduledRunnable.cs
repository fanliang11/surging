/*
 * Copyright 2012 The Netty Project
 *
 * The Netty Project licenses this file to you under the Apache License,
 * version 2.0 (the "License"); you may not use this file except in compliance
 * with the License. You may obtain a copy of the License at:
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * Copyright (c) The DotNetty Project (Microsoft). All rights reserved.
 *
 *   https://github.com/azure/dotnetty
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 *
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com) All rights reserved.
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Common.Concurrency
{
    using System;
    using System.ComponentModel;
    using DotNetty.Common.Internal;

    public interface IScheduledRunnable : IRunnable, IScheduledTask, IComparable<IScheduledRunnable>, IPriorityQueueNode<IScheduledRunnable>
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        IScheduledRunnable SetId(long id);

        [EditorBrowsable(EditorBrowsableState.Never)]
        void SetConsumed();

        [EditorBrowsable(EditorBrowsableState.Never)]
        bool CancelWithoutRemove();
    }
}