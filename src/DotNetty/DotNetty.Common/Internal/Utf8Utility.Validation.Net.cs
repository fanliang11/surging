// borrowed from https://github.com/dotnet/corefx/tree/release/3.1/src/Common/src/CoreLib/System/Text/Unicode

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if NET
using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

namespace DotNetty.Common.Internal
{
    partial class Utf8Utility
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong GetNonAsciiBytes(Vector128<byte> value, Vector128<byte> bitMask128)
        {
            if (!AdvSimd.Arm64.IsSupported || !BitConverter.IsLittleEndian)
            {
                throw ThrowHelper.GetNotSupportedException(); ;
            }

            Vector128<byte> mostSignificantBitIsSet = AdvSimd.ShiftRightArithmetic(value.AsSByte(), 7).AsByte();
            Vector128<byte> extractedBits = AdvSimd.And(mostSignificantBitIsSet, bitMask128);
            extractedBits = AdvSimd.Arm64.AddPairwise(extractedBits, extractedBits);
            return extractedBits.AsUInt64().ToScalar();
        }
    }
}
#endif
