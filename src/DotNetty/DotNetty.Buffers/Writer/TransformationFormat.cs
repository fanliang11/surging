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
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com)
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//
// borrowed from https://github.com/dotnet/corefxlab/tree/master/src/System.Buffers.ReaderWriter/System/Buffers/Writer

namespace DotNetty.Buffers
{
    using System;
    using System.Buffers;
    using System.Runtime.CompilerServices;

    public readonly struct TransformationFormat
    {
        private readonly IBufferTransformation _first;
        private readonly IBufferTransformation[] _rest;

        public TransformationFormat(IBufferTransformation transformation)
        {
            Format = default;
            _first = transformation;
            _rest = null;
        }

        public TransformationFormat(params IBufferTransformation[] transformations)
        {
            Format = default;
            _first = null;
            _rest = transformations;
        }

        public StandardFormat Format { get; }

        public bool TryTransform(Span<byte> buffer, ref int bytesWritten)
        {
            if (_first is object)
            {
                var status = _first.Transform(buffer, bytesWritten, out int transformed);
                switch (status)
                {
                    case OperationStatus.Done:
                        bytesWritten = transformed;
                        return true;

                    case OperationStatus.DestinationTooSmall:
                        return false;

                    case OperationStatus.NeedMoreData:
                    case OperationStatus.InvalidData:
                    default:
                        Throw(status); break;
                }
            }

            if (_rest is object)
            {
                return TryTransformMulti(buffer, ref bytesWritten);
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool TryTransformMulti(Span<byte> buffer, ref int bytesWritten)
        {
            var transformed = bytesWritten;
            for (int i = 0; i < _rest.Length; i++)
            {
                var status = _rest[i].Transform(buffer, transformed, out transformed);

                switch (status)
                {
                    case OperationStatus.Done:
                        continue;

                    case OperationStatus.DestinationTooSmall:
                        return false;

                    case OperationStatus.NeedMoreData:
                    case OperationStatus.InvalidData:
                    default:
                        Throw(status); break;
                }
            }

            bytesWritten = transformed;
            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void Throw(OperationStatus status)
        {
            throw GetException();
            InvalidOperationException GetException()
            {
                return new InvalidOperationException(status.ToString());
            }
        }
    }
}
