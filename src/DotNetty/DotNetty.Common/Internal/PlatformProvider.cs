// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Common.Internal
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public static class PlatformProvider
    {
        static IPlatform defaultPlatform;

        public static IPlatform Platform
        {
            [MethodImpl(InlineMethod.AggressiveInlining)]
            get => Volatile.Read(ref defaultPlatform) ?? EnsurePlatformCreated();
            set
            {
                if (value is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value); }
                _ = Interlocked.Exchange(ref defaultPlatform, value);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IPlatform EnsurePlatformCreated()
        {
            var platform = new DefaultPlatform();
            IPlatform current = Interlocked.CompareExchange(ref defaultPlatform, platform, null);
            return current ?? platform;
        }
    }
}