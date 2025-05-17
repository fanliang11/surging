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
    using System.Runtime.CompilerServices;

    public static class AsciiStringExtensions
    {
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static byte[] ToByteArray(this AsciiString ascii) => ascii.ToByteArray(0, ascii.Count);

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static char[] ToCharArray(this AsciiString ascii) => ascii.ToCharArray(0, ascii.Count);

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static bool Contains(this AsciiString ascii, ICharSequence sequence) => (SharedConstants.TooBigOrNegative >= (uint)ascii.IndexOf(sequence)) ? true : false;

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static int IndexOf(this AsciiString ascii, ICharSequence sequence) => ascii.IndexOf(sequence, 0);

        // Use count instead of count - 1 so lastIndexOf("") answers count
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static int LastIndexOf(this AsciiString ascii, ICharSequence charSequence) => ascii.LastIndexOf(charSequence, ascii.Count);

        public static bool EndsWith(this AsciiString ascii, ICharSequence suffix)
        {
            int suffixLen = suffix.Count;
            return ascii.RegionMatches(ascii.Count - suffixLen, suffix, 0, suffixLen) ? true : false;
        }
    }
}
