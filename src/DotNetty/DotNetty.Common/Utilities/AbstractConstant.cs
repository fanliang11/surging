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

namespace DotNetty.Common.Utilities
{
    using System;
    using System.Threading;

    public abstract class AbstractConstant : IConstant
    {
        static long s_nextUniquifier;

        long v_uniquifier;

        protected AbstractConstant(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; }

        public string Name { get; }

        public sealed override string ToString() => Name;

        protected long Uniquifier
        {
            get
            {
                long result;
                if ((result = Volatile.Read(ref v_uniquifier)) == SharedConstants.Zero64)
                {
                    result = Interlocked.Increment(ref s_nextUniquifier);
                    long previousUniquifier = Interlocked.CompareExchange(ref v_uniquifier, result, SharedConstants.Zero64);
                    if (previousUniquifier != SharedConstants.Zero64)
                    {
                        result = previousUniquifier;
                    }
                }

                return result;
            }
        }

        public abstract bool Equals(IConstant other);
    }

    /// <summary>Base implementation of <see cref="IConstant" />.</summary>
    public abstract class AbstractConstant<T> : AbstractConstant, IComparable<T>, IEquatable<T>
        where T : AbstractConstant<T>
    {
        /// <summary>Creates a new instance.</summary>
        protected AbstractConstant(int id, string name)
            : base(id, name)
        {
        }

        public sealed override int GetHashCode() => base.GetHashCode();

        public sealed override bool Equals(object obj) => base.Equals(obj);

        public bool Equals(T other) => ReferenceEquals(this, other);
        public override bool Equals(IConstant other) => ReferenceEquals(this, other);

        public int CompareTo(T o)
        {
            if (ReferenceEquals(this, o))
            {
                return 0;
            }

            AbstractConstant<T> other = o;

            int returnCode = GetHashCode() - other.GetHashCode();
            if (returnCode != 0)
            {
                return returnCode;
            }

            long thisUV = Uniquifier;
            long otherUV = other.Uniquifier;
            if (thisUV < otherUV)
            {
                return -1;
            }
            if (thisUV > otherUV)
            {
                return 1;
            }

            return ThrowHelper.FromException_CompareConstant();
        }
    }
}