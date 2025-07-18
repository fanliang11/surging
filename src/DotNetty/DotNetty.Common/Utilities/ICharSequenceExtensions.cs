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

namespace DotNetty.Common.Utilities
{
    using System.Runtime.CompilerServices;
#if NET
    using System;
#else
    using System.Runtime.InteropServices;
    using DotNetty.Common.Internal;
#endif


    public static class ICharSequenceExtensions
    {
        public static bool Contains(this ICharSequence sequence, char c)
        {
            switch (sequence)
            {
                case null:
                    return false;

                case IHasAsciiSpan hasAscii:
                    if ((uint)c > AsciiString.uMaxCharValue) { return false; }
#if NET
                    return hasAscii.AsciiSpan.Contains((byte)c);
#else
                    var asciiSpan = hasAscii.AsciiSpan;
                    return SpanHelpers.Contains(ref MemoryMarshal.GetReference(asciiSpan), (byte)c, asciiSpan.Length);
#endif

                case IHasUtf16Span hasUtf16:
#if NET
#else
                    var utf16Span = hasUtf16.Utf16Span;
                    return SpanHelpers.Contains(ref MemoryMarshal.GetReference(utf16Span), c, utf16Span.Length);
#endif

                default:
                    int length = sequence.Count;
                    for (int i = 0; i < length; i++)
                    {
                        if (sequence[i] == c)
                        {
                            return true;
                        }
                    }
                    return false;
            }
        }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static bool StartsWith(this ICharSequence seq, ICharSequence prefix)
            => seq.StartsWith(prefix, 0) ? true : false;

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static bool StartsWith(this ICharSequence seq, ICharSequence prefix, int start)
            => seq.RegionMatches(start, prefix, 0, prefix.Count) ? true : false;
    }
}