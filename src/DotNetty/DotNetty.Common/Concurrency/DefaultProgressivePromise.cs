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
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com) All rights reserved.
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Common.Concurrency
{
    using System;

    public class DefaultProgressivePromise : DefaultPromise, IProgressivePromise
    {
        public DefaultProgressivePromise() : base() { }

        public DefaultProgressivePromise(object state) : base(state) { }

        public void SetProgress(long progress, long total)
        {
            if (total < 0)
            {
                // total unknown
                total = -1; // normalize
                if (progress < 0)
                {
                    throw new ArgumentException("progress: " + progress + " (expected: >= 0)");
                }
            }
            else if (progress < 0 || progress > total)
            {
                throw new ArgumentException(
                        "progress: " + progress + " (expected: 0 <= progress <= total (" + total + "))");
            }

            if (this.IsCompleted)
            {
                throw new InvalidOperationException("complete already");
            }

            //notifyProgressiveListeners(progress, total);
        }

        public bool TryProgress(long progress, long total)
        {
            if (total < 0)
            {
                total = -1;
                if (progress < 0 || this.IsCompleted)
                {
                    return false;
                }
            }
            else if (progress < 0 || progress > total || this.IsCompleted)
            {
                return false;
            }

            //notifyProgressiveListeners(progress, total);
            return true;
        }
    }
}
