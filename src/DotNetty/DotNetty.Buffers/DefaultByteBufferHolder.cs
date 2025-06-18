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
 * Copyright (c) Microsoft. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 *
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com)
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Buffers
{
    using System;
    using DotNetty.Common;

    public class DefaultByteBufferHolder : IByteBufferHolder, IEquatable<IByteBufferHolder>
    {
        private readonly IByteBuffer _data;

        public DefaultByteBufferHolder(IByteBuffer data)
        {
            if (data is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.data); }

            _data = data;
        }

        public IByteBuffer Content
        {
            get
            {
                var refCnt = _data.ReferenceCount;
                if ((uint)(refCnt - 1) > SharedConstants.TooBigOrNegative) // <= 0
                {
                    ThrowHelper.ThrowIllegalReferenceCountException(refCnt);
                }

                return _data;
            }
        }

        public virtual IByteBufferHolder Copy() => Replace(_data.Copy());

        public virtual IByteBufferHolder Duplicate() => Replace(_data.Duplicate());

        public virtual IByteBufferHolder RetainedDuplicate() => Replace(_data.RetainedDuplicate());

        public virtual IByteBufferHolder Replace(IByteBuffer content) => new DefaultByteBufferHolder(content);

        public virtual int ReferenceCount => _data.ReferenceCount;

        public IReferenceCounted Retain()
        {
            _ = _data.Retain();
            return this;
        }

        public IReferenceCounted Retain(int increment)
        {
            _ = _data.Retain(increment);
            return this;
        }

        public IReferenceCounted Touch()
        {
            _ = _data.Touch();
            return this;
        }

        public IReferenceCounted Touch(object hint)
        {
            _ = _data.Touch(hint);
            return this;
        }

        public bool Release() => _data.Release();

        public bool Release(int decrement) => _data.Release(decrement);

        protected string ContentToString() => _data.ToString();

        /// <summary>
        /// This implementation of the <see cref="Equals(object)"/> operation is restricted to
        /// work only with instances of the same class. The reason for that is that
        /// Netty library already has a number of classes that extend <see cref="DefaultByteBufferHolder"/> and
        /// override <see cref="Equals(object)"/> method with an additional comparison logic and we
        /// need the symmetric property of the <see cref="Equals(object)"/> operation to be preserved.
        /// </summary>
        /// <param name="obj">the reference object with which to compare.</param>
        /// <returns><c>true</c> if this object is the same as the <paramref name="obj"/>
        /// argument; <c>false</c> otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) { return true; }

            return obj is object && GetType() == obj.GetType() && _data.Equals(((DefaultByteBufferHolder)obj)._data);
        }

        /// <summary>
        /// This implementation of the <see cref="Equals(IByteBufferHolder)"/> operation is restricted to
        /// work only with instances of the same class. The reason for that is that
        /// Netty library already has a number of classes that extend <see cref="DefaultByteBufferHolder"/> and
        /// override <see cref="Equals(IByteBufferHolder)"/> method with an additional comparison logic and we
        /// need the symmetric property of the <see cref="Equals(IByteBufferHolder)"/> operation to be preserved.
        /// </summary>
        /// <param name="other">the reference object with which to compare.</param>
        /// <returns><c>true</c> if this object is the same as the <paramref name="other"/>
        /// argument; <c>false</c> otherwise.</returns>
        public bool Equals(IByteBufferHolder other) => Equals(obj: other);

        public override int GetHashCode() => _data.GetHashCode();
    }
}
