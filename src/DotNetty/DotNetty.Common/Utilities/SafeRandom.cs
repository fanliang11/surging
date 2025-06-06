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
    using System;
    using System.Runtime.CompilerServices;
    using System.Security.Cryptography;

    /// <summary>
    /// Thread-safe random number generator.
    /// Has same API as System.Random but is thread safe, similar to the implementation by Steven Toub: http://blogs.msdn.com/b/pfxteam/archive/2014/10/20/9434171.aspx
    /// </summary>
    public sealed class SafeRandom
    {
        private static readonly RandomNumberGenerator s_globalCryptoProvider = RandomNumberGenerator.Create();

        [ThreadStatic]
        private static Random random;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Random GetRandom()
        {
            if (random == null)
            {
                byte[] buffer = new byte[4];
                s_globalCryptoProvider.GetBytes(buffer);
                random = new Random(BitConverter.ToInt32(buffer, 0));
            }

            return random;
        }

        public int Next()
        {
            return GetRandom().Next();
        }

        public int Next(int maxValue)
        {
            return GetRandom().Next(maxValue);
        }

        public int Next(int minValue, int maxValue)
        {
            return GetRandom().Next(minValue, maxValue);
        }

        public void NextBytes(byte[] buffer)
        {
            GetRandom().NextBytes(buffer);
        }

        public double NextDouble()
        {
            return GetRandom().NextDouble();
        }
    }
}
