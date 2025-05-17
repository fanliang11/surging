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

namespace DotNetty.Codecs.Http
{
    public class DefaultHttpObject : IHttpObject
    {
        const int HashCodePrime = 31;
        DecoderResult decoderResult = DecoderResult.Success;

        protected DefaultHttpObject()
        {
        }

        public DecoderResult Result
        {
            get => this.decoderResult;
            set
            {
                if (value is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value); }
                this.decoderResult = value;
            }
        }

        public override int GetHashCode()
        {
            int result = 1;
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            result = HashCodePrime * result + this.decoderResult.GetHashCode();
            return result;
        }

        public override bool Equals(object obj)
        {
            if (obj is DefaultHttpObject other)
            {
                return this.decoderResult.Equals(other.decoderResult);
            }
            return false;
        }
    }
}
