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

    public static class MediumUtil
    {
        //High byte bit-mask used when a 24-bit integer is stored within a 32-bit integer.
        const uint MediumBitMask = 0xff000000;

        public static int ToMediumInt(this int value)
        {
            // Check bit 23, the sign bit in a signed 24-bit integer
            if ((value & 0x00800000) > 0)
            {
                // If the sign-bit is set, this number will be negative - set all high-byte bits (keeps 32-bit number in 24-bit range)
                value |= unchecked((int)MediumBitMask);
            }
            else
            {
                // If the sign-bit is not set, this number will be positive - clear all high-byte bits (keeps 32-bit number in 24-bit range)
                value &= ~unchecked((int)MediumBitMask);
            }

            return value;
        }

        public static int ToUnsignedMediumInt(this int value)
        {
            return (int)((uint)value & ~MediumBitMask);
        }
    }
}
