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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides the ability to associate the outcome of multiple <see cref="IPromise"/>
    /// objects into a single <see cref="IPromise"/> object.
    /// </summary>
    public sealed class SimplePromiseAggregator : DefaultValueTaskPromise
    {
        readonly IPromise promise;
        int expectedCount;
        int doneCount;
        bool doneAllocating;

        bool hasFailure;
        Exception lastFailure;
        IEnumerable<Exception> lastFailures;

        public SimplePromiseAggregator(IPromise promise)
        {
            Debug.Assert(promise is object && !promise.IsCompleted);
            this.promise = promise;
        }

        /// <summary>
        /// Allocate a new promise which will be used to aggregate the overall success of this promise aggregator.
        /// </summary>
        /// <returns>A new promise which will be aggregated.
        /// <c>null</c> if <see cref="DoneAllocatingPromises()"/> was previously called.</returns>
        public IPromise NewPromise()
        {
            Debug.Assert(!this.doneAllocating, "Done allocating. No more promises can be allocated.");
            ++this.expectedCount;
            return this;
        }

        /// <summary>
        /// Signify that no more <see cref="NewPromise()"/> allocations will be made.
        /// The aggregation can not be successful until this method is called.
        /// </summary>
        /// <returns>The promise that is the aggregation of all promises allocated with <see cref="NewPromise()"/>.</returns>
        public IPromise DoneAllocatingPromises()
        {
            if (!this.doneAllocating)
            {
                this.doneAllocating = true;
                if (this.doneCount == this.expectedCount || 0u >= (uint)this.expectedCount)
                {
                    this.SetPromise();
                }
            }
            return this;
        }

        public override bool TrySetException(Exception cause)
        {
            if (this.AllowFailure())
            {
                ++this.doneCount;
                this.hasFailure = true;
                this.lastFailure = cause;
                if (this.AllPromisesDone())
                {
                    return this.TryPromise();
                }

                // TODO: We break the interface a bit here.
                // Multiple failure events can be processed without issue because this is an aggregation.
                return true;
            }

            return false;
        }

        public override bool TrySetException(IEnumerable<Exception> ex)
        {
            if (this.AllowFailure())
            {
                ++this.doneCount;
                this.hasFailure = true;
                this.lastFailures = ex;
                if (this.AllPromisesDone())
                {
                    return this.TryPromise();
                }

                // TODO: We break the interface a bit here.
                // Multiple failure events can be processed without issue because this is an aggregation.
                return true;
            }

            return false;
        }

        /// <summary>
        /// Fail this object if it has not already been failed.
        /// 
        /// This method will NOT throw an <see cref="InvalidOperationException"/> if called multiple times
        /// because that may be expected.
        /// </summary>
        /// <param name="cause"></param>
        public override void SetException(Exception cause)
        {
            if (this.AllowFailure())
            {
                ++this.doneCount;
                this.hasFailure = true;
                this.lastFailure = cause;
                if (this.AllPromisesDone())
                {
                    this.SetPromise();
                }
            }
        }

        public override void SetException(IEnumerable<Exception> ex)
        {
            if (this.AllowFailure())
            {
                ++this.doneCount;
                this.hasFailure = true;
                this.lastFailures = ex;
                if (this.AllPromisesDone())
                {
                    this.SetPromise();
                }
            }
        }

        public override bool TrySetCanceled() => false;

        public override void SetCanceled() { }

        public override void Complete()
        {
            if (this.AwaitingPromises())
            {
                ++this.doneCount;
                if (this.AllPromisesDone())
                {
                    this.SetPromise();
                }
            }
        }

        public override bool TryComplete()
        {
            if (this.AwaitingPromises())
            {
                ++this.doneCount;
                if (this.AllPromisesDone())
                {
                    return this.TryPromise();
                }

                // TODO: We break the interface a bit here.
                // Multiple success events can be processed without issue because this is an aggregation.
                return true;
            }

            return false;
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        bool AllowFailure()
        {
            return this.AwaitingPromises() || 0u >= (uint)this.expectedCount;
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        bool AwaitingPromises()
        {
            return this.doneCount < this.expectedCount;
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        bool AllPromisesDone()
        {
            return (this.doneCount == this.expectedCount) && this.doneAllocating;
        }

        void SetPromise()
        {
            if (!this.hasFailure)
            {
                this.promise.Complete();
                base.Complete();
                return;
            }
            if (this.lastFailure is object)
            {
                this.promise.SetException(this.lastFailure);
                base.SetException(this.lastFailure);
                return;
            }

            this.promise.SetException(this.lastFailures);
            base.SetException(this.lastFailures);
        }

        bool TryPromise()
        {
            if (!this.hasFailure)
            {
                _ = this.promise.TryComplete();
                return base.TryComplete();
            }

            if (this.lastFailure is object)
            {
                _ = this.promise.TrySetException(this.lastFailure);
                return base.TrySetException(this.lastFailure);
            }

            _ = this.promise.TrySetException(this.lastFailures);
            return base.TrySetException(this.lastFailures);
        }
    }
}