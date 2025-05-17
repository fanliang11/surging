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

namespace DotNetty.Common
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    public readonly struct PreciseTimeSpan : IComparable<PreciseTimeSpan>, IEquatable<PreciseTimeSpan>
    {
        public static readonly PreciseTimeSpan Zero = new PreciseTimeSpan(0);

        public static readonly PreciseTimeSpan MinusOne = new PreciseTimeSpan(-1);

        private readonly long _ticks;

        [MethodImpl(InlineMethod.AggressiveInlining)]
        PreciseTimeSpan(long ticks)
            : this()
        {
            _ticks = ticks;
        }

        public readonly long Ticks => _ticks;

        public TimeSpan ToTimeSpan() => TimeSpan.FromTicks((long)(_ticks * PreciseTime.ReversePrecisionRatio));


        public readonly bool Equals(PreciseTimeSpan other) => 0UL >= (ulong)(_ticks - other._ticks); // _ticks == other._ticks;

        public override readonly bool Equals(object obj)
        {
            return obj is PreciseTimeSpan preciseTimeSpan && Equals(preciseTimeSpan);
        }

        public override int GetHashCode() => _ticks.GetHashCode();

        public readonly int CompareTo(PreciseTimeSpan other) => _ticks.CompareTo(other._ticks);


        public static PreciseTimeSpan FromTicks(long preciseTicks) => new PreciseTimeSpan(preciseTicks);

        public static PreciseTimeSpan FromStart => new PreciseTimeSpan(PreciseTime.NanoTime());

        public static PreciseTimeSpan FromTimeSpan(TimeSpan timeSpan) => new PreciseTimeSpan(PreciseTime.TicksToPreciseTicks(timeSpan.Ticks));

        public static PreciseTimeSpan Deadline(TimeSpan deadline) => new PreciseTimeSpan(PreciseTime.NanoTime() + PreciseTime.TicksToPreciseTicks(deadline.Ticks));

        public static PreciseTimeSpan Deadline(in PreciseTimeSpan deadline) => new PreciseTimeSpan(PreciseTime.NanoTime() + deadline._ticks);

        public static bool operator ==(PreciseTimeSpan t1, PreciseTimeSpan t2) => 0UL >= (ulong)(t1._ticks - t2._ticks); // t1._ticks == t2._ticks;

        public static bool operator !=(PreciseTimeSpan t1, PreciseTimeSpan t2) => (ulong)(t1._ticks - t2._ticks) > 0UL; // t1._ticks != t2._ticks;

        public static bool operator >(PreciseTimeSpan t1, PreciseTimeSpan t2) => t1._ticks > t2._ticks;

        public static bool operator <(PreciseTimeSpan t1, PreciseTimeSpan t2) => t1._ticks < t2._ticks;

        public static bool operator >=(PreciseTimeSpan t1, PreciseTimeSpan t2) => t1._ticks >= t2._ticks;

        public static bool operator <=(PreciseTimeSpan t1, PreciseTimeSpan t2) => t1._ticks <= t2._ticks;

        public static PreciseTimeSpan operator +(PreciseTimeSpan t, TimeSpan duration)
        {
            long ticks = t._ticks + PreciseTime.TicksToPreciseTicks(duration.Ticks);
            return new PreciseTimeSpan(ticks);
        }

        public static PreciseTimeSpan operator -(PreciseTimeSpan t, TimeSpan duration)
        {
            long ticks = t._ticks - PreciseTime.TicksToPreciseTicks(duration.Ticks);
            return new PreciseTimeSpan(ticks);
        }

        public static PreciseTimeSpan operator -(PreciseTimeSpan t1, PreciseTimeSpan t2)
        {
            long ticks = t1._ticks - t2._ticks;
            return new PreciseTimeSpan(ticks);
        }
    }

    public static class PreciseTime
    {
        public const long Zero = 0L;
        public const long MinusOne = -1L;

        private const long TicksPerMillisecond = TimeSpan.TicksPerMillisecond;
        private const double MillisecondsPerTick = 1.0 / TicksPerMillisecond;
        private const long MaxMilliSeconds = long.MaxValue / TicksPerMillisecond;
        private const long MinMilliSeconds = long.MinValue / TicksPerMillisecond;

        /// <summary>The initial value used for delay and computations based upon a monatomic time source.</summary>
        public static readonly long StartTime;

        private static readonly double PrecisionRatio;
        internal static readonly double ReversePrecisionRatio;

        static PreciseTime()
        {
            StartTime = Stopwatch.GetTimestamp();
            PrecisionRatio = (double)Stopwatch.Frequency / TimeSpan.TicksPerSecond;
            ReversePrecisionRatio = (double)TimeSpan.TicksPerSecond / Stopwatch.Frequency;
        }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static long NanoTime() => Stopwatch.GetTimestamp() - StartTime;

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static long DeadlineNanos(TimeSpan deadline) => NanoTime() + TicksToPreciseTicks(deadline.Ticks);

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static long DeadlineNanos(long delayNanos)
        {
            long deadlineNanos = unchecked(NanoTime() + delayNanos);
            // Guard against overflow
            if (SharedConstants.TooBigOrNegative64 >= (ulong)deadlineNanos)
            {
                return deadlineNanos;
            }
            return long.MaxValue;
        }

        /// <summary>
        /// Given an arbitrary deadline <paramref name="deadlineNanos"/>, calculate the number of nano seconds from now
        /// <paramref name="deadlineNanos"/> would expire.
        /// </summary>
        /// <param name="deadlineNanos">An arbitrary deadline in nano seconds.</param>
        /// <returns>the number of nano seconds from now <paramref name="deadlineNanos"/> would expire.</returns>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static long DeadlineToDelayNanos(long deadlineNanos)
        {
            if (0ul >= (ulong)deadlineNanos) { return 0L; }

            return Math.Max(0L, deadlineNanos - NanoTime());
        }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static long ToDelayNanos(TimeSpan timeSpan) => TicksToPreciseTicks(timeSpan.Ticks);

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static TimeSpan ToTimeSpan(long delayNanos) => TimeSpan.FromTicks((long)(delayNanos * ReversePrecisionRatio));

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static long ToMilliseconds(long delayNanos)
        {
            var ticks = (long)(delayNanos * ReversePrecisionRatio);
            double temp = (double)ticks * MillisecondsPerTick;

            if (temp > MaxMilliSeconds) { return MaxMilliSeconds; }
            if (temp < MinMilliSeconds) { return MinMilliSeconds; }

            return (long)temp;
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static long TicksToPreciseTicks(long ticks)
        {
            if (Stopwatch.IsHighResolution)
            {
                return (long)(ticks * PrecisionRatio);
            }
            return ticks;
        }
    }
}