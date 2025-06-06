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

namespace DotNetty
{
    public static class SharedConstants
    {
        public const int True = 1;

        public const int False = 0;

        public const int Zero = 0;

        public const byte Zero8 = 0;

        public const long Zero64 = 0L;

        public const int IndexNotFound = -1;
        public const uint uIndexNotFound = unchecked((uint)IndexNotFound);

        public const uint TooBigOrNegative = int.MaxValue;
        public const ulong TooBigOrNegative64 = long.MaxValue;

        public const uint uStackallocThreshold = 256u;
        public const int StackallocThreshold = 256;
    }
}
