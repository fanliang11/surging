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
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Internal.Logging;
    using DotNetty.Common.Utilities;

    public class ResourceLeakDetector
    {
        private const string PropLevel = "io.netty.leakDetection.level";
        private const DetectionLevel DefaultLevel = DetectionLevel.Simple;

        private const string PropTargetRecords = "io.netty.leakDetection.targetRecords";
        private const int DefaultTargetRecords = 4;

        private const string PropSamplingInterval = "io.netty.leakDetection.samplingInterval";
        // There is a minor performance benefit in TLR if this is a power of 2.
        private const int DefaultSamplingInterval = 128;

        private static readonly int s_targetRecords;
        private static readonly int s_samplingInterval;

        /// <summary>
        /// Represents the level of resource leak detection.
        /// </summary>
        public enum DetectionLevel
        {
            /// <summary>
            /// Disables resource leak detection.
            /// </summary>
            Disabled,

            /// <summary>
            /// Enables simplistic sampling resource leak detection which reports there is a leak or not,
            /// at the cost of small overhead (default).
            /// </summary>
            Simple,

            /// <summary>
            /// Enables advanced sampling resource leak detection which reports where the leaked object was accessed
            /// recently at the cost of high overhead.
            /// </summary>
            Advanced,

            /// <summary>
            /// Enables paranoid resource leak detection which reports where the leaked object was accessed recently,
            /// at the cost of the highest possible overhead (for testing purposes only).
            /// </summary>
            Paranoid
        }

        private static readonly IInternalLogger Logger;

        static ResourceLeakDetector()
        {
            Logger = InternalLoggerFactory.GetInstance<ResourceLeakDetector>();

            bool disabled = false;
            if (SystemPropertyUtil.Get("io.netty.noResourceLeakDetection") is object)
            {
                disabled = SystemPropertyUtil.GetBoolean("io.netty.noResourceLeakDetection", false);
                if (Logger.DebugEnabled) { Logger.Debug("-Dio.netty.noResourceLeakDetection: {}", disabled); }
                Logger.Warn(
                        "-Dio.netty.noResourceLeakDetection is deprecated. Use '-D{}={}' instead.",
                        PropLevel, DefaultLevel.ToString().ToLowerInvariant());
            }

            var defaultLevel = disabled ? DetectionLevel.Disabled : DefaultLevel;

            // If new property name is present, use it
            string levelStr = SystemPropertyUtil.Get(PropLevel, defaultLevel.ToString());
            if (!Enum.TryParse(levelStr, true, out DetectionLevel level))
            {
                level = defaultLevel;
            }

            s_targetRecords = SystemPropertyUtil.GetInt(PropTargetRecords, DefaultTargetRecords);
            s_samplingInterval = SystemPropertyUtil.GetInt(PropSamplingInterval, DefaultSamplingInterval);
            Level = level;

            if (Logger.DebugEnabled)
            {
                Logger.Debug("-D{}: {}", PropLevel, level.ToString().ToLower());
                Logger.Debug("-D{}: {}", PropTargetRecords, s_targetRecords);
            }
        }

        /// Returns <c>true</c> if resource leak detection is enabled.
        public static bool Enabled => Level > DetectionLevel.Disabled;

        /// <summary>
        /// Gets or sets resource leak detection level
        /// </summary>
        public static DetectionLevel Level { get; set; }

        private readonly ConditionalWeakTable<object, GCNotice> _gcNotificationMap = new ConditionalWeakTable<object, GCNotice>();
        private readonly ConcurrentDictionary<string, bool> _reportedLeaks = new ConcurrentDictionary<string, bool>(StringComparer.Ordinal);

        private readonly string _resourceType;
        private readonly int _samplingInterval;

        public ResourceLeakDetector(string resourceType)
            : this(resourceType, s_samplingInterval)
        {
        }

        public ResourceLeakDetector(string resourceType, int samplingInterval)
        {
            if (resourceType is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.resourceType); }
            if ((uint)(samplingInterval - 1) > SharedConstants.TooBigOrNegative) // <= 0
            {
                ThrowHelper.ThrowArgumentException_Positive(samplingInterval, ExceptionArgument.samplingInterval);
            }

            _resourceType = resourceType;
            _samplingInterval = samplingInterval;
        }

        public static ResourceLeakDetector Create<T>() => new ResourceLeakDetector(StringUtil.SimpleClassName<T>());

        /// <summary>
        ///     Creates a new <see cref="IResourceLeakTracker" /> which is expected to be closed
        ///     when the
        ///     related resource is deallocated.
        /// </summary>
        /// <returns>the <see cref="IResourceLeakTracker" /> or <c>null</c></returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public IResourceLeakTracker Track(object obj)
        {
            DetectionLevel level = Level;
            if (level == DetectionLevel.Disabled)
            {
                return null;
            }

            if (level < DetectionLevel.Paranoid)
            {
                if (0u >= (uint)(PlatformDependent.GetThreadLocalRandom().Next(_samplingInterval)))
                {
                    return new DefaultResourceLeak(this, obj);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return new DefaultResourceLeak(this, obj);
            }
        }

        private void ReportLeak(DefaultResourceLeak resourceLeak)
        {
            if (!Logger.ErrorEnabled)
            {
                resourceLeak.Dispose();
                return;
            }

            string records = resourceLeak.Dump();
            if (_reportedLeaks.TryAdd(records, true))
            {
                if (0u >= (uint)records.Length)
                {
                    ReportUntracedLeak(_resourceType);
                }
                else
                {
                    ReportTracedLeak(_resourceType, records);
                }
            }
        }

        protected void ReportTracedLeak(string type, string records)
        {
            Logger.Error(
                "LEAK: {}.Release() was not called before it's garbage-collected. " +
                "See http://netty.io/wiki/reference-counted-objects.html for more information.{}",
                type, records);
        }

        protected void ReportUntracedLeak(string type)
        {
            Logger.Error("LEAK: {}.release() was not called before it's garbage-collected. " +
                "Enable advanced leak reporting to find out where the leak occurred. " +
                "To enable advanced leak reporting, " +
                "specify the JVM option '-D{}={}' or call {}.setLevel() " +
                "See http://netty.io/wiki/reference-counted-objects.html for more information.",
                type, PropLevel, DetectionLevel.Advanced.ToString().ToLower(), StringUtil.SimpleClassName(this));
        }

        sealed class DefaultResourceLeak : IResourceLeakTracker
        {
            private readonly ResourceLeakDetector _owner;

            private RecordEntry v_head;
            private long _droppedRecords;
            private readonly WeakReference<GCNotice> _gcNotice;

            [MethodImpl(MethodImplOptions.NoInlining)]
            public DefaultResourceLeak(ResourceLeakDetector owner, object referent)
            {
                Debug.Assert(referent is object);

                _owner = owner;
                GCNotice gcNotice;
                do
                {
                    GCNotice gcNotice0 = null;
                    gcNotice = owner._gcNotificationMap.GetValue(referent, referent0 =>
                    {
                        gcNotice0 = new GCNotice(referent0, owner);
                        return gcNotice0;
                    });
                    if (gcNotice0 is object && gcNotice0 != gcNotice)
                    {
                        GC.SuppressFinalize(gcNotice0);
                    }
                }
                while (!gcNotice.Arm(this, owner, referent));
                _gcNotice = new WeakReference<GCNotice>(gcNotice);
                v_head = RecordEntry.Bottom;
                Record();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Record() => Record0(null);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Record(object hint) => Record0(hint);

            [MethodImpl(MethodImplOptions.NoInlining)]
            private void Record0(object hint)
            {
                // Check TARGET_RECORDS > 0 here to avoid similar check before remove from and add to lastRecords
                if (s_targetRecords <= 0) { return; }

                StackTrace stackTrace = null;

                var thisHead = Volatile.Read(ref v_head);
                RecordEntry oldHead;
                RecordEntry prevHead;
                RecordEntry newHead;
                bool dropped;
                do
                {
                    if ((prevHead = oldHead = thisHead) is null)
                    {
                        // already closed.
                        return;
                    }
                    int numElements = thisHead.Pos + 1;
                    if (numElements >= s_targetRecords)
                    {
                        int backOffFactor = Math.Min(numElements - s_targetRecords, 30);
                        dropped = PlatformDependent.GetThreadLocalRandom().Next(1 << backOffFactor) != 0;
                        if (dropped)
                        {
                            prevHead = thisHead.Next;
                        }
                    }
                    else
                    {
                        dropped = false;
                    }
                    stackTrace ??= new StackTrace(skipFrames: 3, fNeedFileInfo: true);
                    newHead = hint is object ? new RecordEntry(prevHead, stackTrace, hint) : new RecordEntry(prevHead, stackTrace);
                    thisHead = Interlocked.CompareExchange(ref v_head, newHead, thisHead);
                }
                while (thisHead != oldHead);
                if (dropped)
                {
                    _ = Interlocked.Increment(ref _droppedRecords);
                }
            }

            public bool Close(object trackedObject)
            {
                if (_gcNotice.TryGetTarget(out var notice))
                {
                    if (notice.UnArm(this, _owner, trackedObject))
                    {
                        Dispose();
                        return true;
                    }
                }

                return false;
            }

            // This is called from GCNotice finalizer
            internal void CloseFinal()
            {
                if (Volatile.Read(ref v_head) is object)
                {
                    _owner.ReportLeak(this);
                }
            }

            public string Dump()
            {
                RecordEntry oldHead = Interlocked.Exchange(ref v_head, null);
                if (oldHead is null)
                {
                    // Already closed
                    return string.Empty;
                }

                long dropped = Interlocked.Read(ref _droppedRecords);
                int duped = 0;

                int present = oldHead.Pos + 1;
                // Guess about 2 kilobytes per stack trace
                var buf = new StringBuilder(present * 2048);
                _ = buf.Append(StringUtil.Newline);
                _ = buf.Append("Recent access records: ").Append(StringUtil.Newline);

                int i = 1;
                var seen = new HashSet<string>(StringComparer.Ordinal);
                for (; oldHead != RecordEntry.Bottom; oldHead = oldHead.Next)
                {
                    string s = oldHead.ToString();
                    if (seen.Add(s))
                    {
                        if (oldHead.Next == RecordEntry.Bottom)
                        {
                            _ = buf.Append("Created at:").Append(StringUtil.Newline).Append(s);
                        }
                        else
                        {
                            _ = buf.Append('#').Append(i++).Append(':').Append(StringUtil.Newline).Append(s);
                        }
                    }
                    else
                    {
                        duped++;
                    }
                }

                if (duped > 0)
                {
                    _ = buf.Append(": ")
                        .Append(duped)
                        .Append(" leak records were discarded because they were duplicates")
                        .Append(StringUtil.Newline);
                }

                if (dropped > 0)
                {
                    _ = buf.Append(": ")
                        .Append(dropped)
                        .Append(" leak records were discarded because the leak record count is targeted to ")
                        .Append(s_targetRecords)
                        .Append(". Use system property ")
                        .Append(PropTargetRecords)
                        .Append(" to increase the limit.")
                        .Append(StringUtil.Newline);
                }

                buf.Length = buf.Length - StringUtil.Newline.Length;
                return buf.ToString();
            }

            internal void Dispose()
            {
                _ = Interlocked.Exchange(ref v_head, null);
            }
        }

        // Record
        sealed class RecordEntry
        {
            internal static readonly RecordEntry Bottom = new RecordEntry();

            internal readonly RecordEntry Next;
            internal readonly int Pos;

            private readonly string _hintString;
            private readonly StackTrace _stackTrace;

            internal RecordEntry(RecordEntry next, StackTrace stackTrace, object hint)
            {
                // This needs to be generated even if toString() is never called as it may change later on.
                _hintString = hint is IResourceLeakHint leakHint ? leakHint.ToHintString() : null;
                Next = next;
                Pos = next.Pos + 1;
                _stackTrace = stackTrace;
            }

            internal RecordEntry(RecordEntry next, StackTrace stackTrace)
            {
                _hintString = null;
                Next = next;
                Pos = next.Pos + 1;
                _stackTrace = stackTrace;
            }

            // Used to terminate the stack
            RecordEntry()
            {
                _hintString = null;
                Next = null;
                Pos = -1;
                _stackTrace = null;
            }

            public override string ToString()
            {
                var buf = new StringBuilder(2048);
                if (_hintString is object)
                {
                    _ = buf.Append("\tHint: ").Append(_hintString).Append(StringUtil.Newline);
                }

                // Append the stack trace.
                _ = AppendStackTrace(buf, _stackTrace).Append(StringUtil.Newline);
                return buf.ToString();
            }

            private static StringBuilder AppendStackTrace(StringBuilder sb, StackTrace stackTrace)
            {
                if (stackTrace == null)
                    return sb;

                // TODO: Support excludedMethods NETStandard2.0
                return sb.Append(stackTrace);
            }
        }

        sealed class GCNotice
        {
            // ConditionalWeakTable
            //
            // Lifetimes of keys and values:
            //
            //    Inserting a key and value into the dictonary will not
            //    prevent the key from dying, even if the key is strongly reachable
            //    from the value.
            //
            //    Prior to ConditionalWeakTable, the CLR did not expose
            //    the functionality needed to implement this guarantee.
            //
            //    Once the key dies, the dictionary automatically removes
            //    the key/value entry.
            //
            private readonly LinkedList<DefaultResourceLeak> _leakList = new LinkedList<DefaultResourceLeak>();
            private object _referent;
            private ResourceLeakDetector _owner;

            public GCNotice(object referent, ResourceLeakDetector owner)
            {
                _referent = referent;
                _owner = owner;
            }

            ~GCNotice()
            {
                lock (_leakList)
                {
                    foreach (var leak in _leakList)
                    {
                        leak.CloseFinal();
                    }
                    _leakList.Clear();

                    //Since we get here with finalizer, it's no needed to remove key from gcNotificationMap

                    //_referent = null;
                    _owner = null;
                }
            }

            public bool Arm(DefaultResourceLeak leak, ResourceLeakDetector owner, object referent)
            {
                lock (_leakList)
                {
                    if (_owner is null)
                    {
                        //Already disposed
                        return false;
                    }
                    Debug.Assert(owner == _owner);
                    Debug.Assert(referent == _referent);

                    _ = _leakList.AddLast(leak);
                    return true;
                }
            }

            public bool UnArm(DefaultResourceLeak leak, ResourceLeakDetector owner, object referent)
            {
                lock (_leakList)
                {
                    if (_owner is null)
                    {
                        //Already disposed
                        return false;
                    }
                    Debug.Assert(owner == _owner);
                    Debug.Assert(referent == _referent);

                    bool res = _leakList.Remove(leak);
                    if (0u >= (uint)_leakList.Count)
                    {
                        // The close is called by byte buffer release, in this case
                        // we suppress the GCNotice finalize to prevent false positive
                        // report where the byte buffer instance gets reused by thread
                        // local cache and the existing GCNotice finalizer still holds
                        // the same byte buffer instance.
                        GC.SuppressFinalize(this);

                        // Don't inline the variable, anything inside Debug.Assert()
                        // will be stripped out in Release builds
                        bool removed = _owner._gcNotificationMap.Remove(_referent);
                        Debug.Assert(removed);

                        //_referent = null;
                        _owner = null;
                    }
                    return res;
                }
            }
        }
    }
}