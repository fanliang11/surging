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


namespace DotNetty.Transport.Channels
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using DotNetty.Buffers;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Internal.Logging;
    using DotNetty.Common;

    sealed class DefaultChannelId : IChannelId, IEquatable<IChannelId>
    {
        const int MachineIdLen = 8;
        const int ProcessIdLen = 4;
        // Maximal value for 64bit systems is 2^22.  See man 5 proc.
        // See https://github.com/netty/netty/issues/2706
        const int MaxProcessId = 4194304;
        const int SequenceLen = 4;
        const int TimestampLen = 8;
        const int RandomLen = 4;
        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<DefaultChannelId>();
        static readonly Regex MachineIdPattern = new Regex("^(?:[0-9a-fA-F][:-]?){6,8}$");
        static readonly byte[] MachineId;
        static readonly int ProcessId;
        static int nextSequence;
        static int seed = (int)(Stopwatch.GetTimestamp() & 0xFFFFFFFF); //used to safly cast long to int, because the timestamp returned is long and it doesn't fit into an int
        static readonly ThreadLocal<Random> ThreadLocalRandom = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref seed))); //used to simulate java ThreadLocalRandom
        readonly byte[] data = new byte[MachineIdLen + ProcessIdLen + SequenceLen + TimestampLen + RandomLen];
        int hashCode;

        string longValue;

        string shortValue;

        static DefaultChannelId()
        {
            int processId = -1;
            string customProcessId = SystemPropertyUtil.Get("io.netty.processId");
            if (customProcessId is object)
            {
                if (!int.TryParse(customProcessId, out processId))
                {
                    processId = -1;
                }
                if (processId < 0 || processId > MaxProcessId)
                {
                    processId = -1;
                    Logger.Warn("-Dio.netty.processId: {} (malformed)", customProcessId);
                }
                else if (Logger.DebugEnabled)
                {
                    Logger.Debug("-Dio.netty.processId: {} (user-set)", processId);
                }
            }
            if (processId < 0)
            {
                processId = DefaultProcessId();
                if (Logger.DebugEnabled)
                {
                    Logger.Debug("-Dio.netty.processId: {} (auto-detected)", processId);
                }
            }
            ProcessId = processId;
            byte[] machineId = null;
            string customMachineId = SystemPropertyUtil.Get("io.netty.machineId");
            if (customMachineId is object)
            {
                if (MachineIdPattern.Match(customMachineId).Success)
                {
                    machineId = ParseMachineId(customMachineId);
                    if (Logger.DebugEnabled) Logger.Debug("-Dio.netty.machineId: {} (user-set)", customMachineId);
                }
                else
                {
                    Logger.Warn("-Dio.netty.machineId: {} (malformed)", customMachineId);
                }
            }

            if (machineId is null)
            {
                machineId = DefaultMachineId();
                if (Logger.DebugEnabled)
                {
                    Logger.Debug("-Dio.netty.machineId: {} (auto-detected)", MacAddressUtil.FormatAddress(machineId));
                }
            }
            MachineId = machineId;
        }

        public string AsShortText()
        {
            string asShortText = this.shortValue;
            if (asShortText is null)
            {
                this.shortValue = asShortText = ByteBufferUtil.HexDump(this.data, MachineIdLen + ProcessIdLen + SequenceLen + TimestampLen, RandomLen);
            }

            return asShortText;
        }

        public string AsLongText()
        {
            string asLongText = this.longValue;
            if (asLongText is null)
            {
                this.longValue = asLongText = this.NewLongValue();
            }
            return asLongText;
        }

        public int CompareTo(IChannelId other)
        {
            if (ReferenceEquals(this, other))
            {
                // short circuit
                return 0;
            }
            if (other is DefaultChannelId otherId)
            {
                // lexicographic comparison
                byte[] otherData = otherId.data;
                int len1 = data.Length;
                int len2 = otherData.Length;
                int len = Math.Min(len1, len2);

                for (int k = 0; k < len; k++)
                {
                    byte x = data[k];
                    byte y = otherData[k];
                    if (x != y)
                    {
                        // treat these as unsigned bytes for comparison
                        return (x & 0xff) - (y & 0xff);
                    }
                }
                return len1 - len2;
            }

            return StringComparer.Ordinal.Compare(AsLongText(), other.AsLongText());

        }

        static byte[] ParseMachineId(string value)
        {
            // Strip separators.
            value = value.Replace("[:-]", "");
            var machineId = new byte[MachineIdLen];
            for (int i = 0; i < value.Length; i += 2)
            {
                machineId[i] = (byte)int.Parse(value.Substring(i, 2), NumberStyles.AllowHexSpecifier);
            }
            return machineId;
        }

        static int DefaultProcessId()
        {
            int pId = Platform.GetCurrentProcessId();

            if ((uint)(pId - 1) > SharedConstants.TooBigOrNegative) // <= 0
            {
                pId = ThreadLocalRandom.Value.Next(MaxProcessId + 1);
            }
            return pId;
        }

        public static DefaultChannelId NewInstance()
        {
            var id = new DefaultChannelId();
            id.Init();
            return id;
        }

        static byte[] DefaultMachineId()
        {
            byte[] bestMacAddr = Platform.GetDefaultDeviceId();
            if (bestMacAddr is null)
            {
                bestMacAddr = new byte[MacAddressUtil.MacAddressLength];
                ThreadLocalRandom.Value.NextBytes(bestMacAddr);
                Logger.Warn(
                    "Failed to find a usable hardware address from the network interfaces; using random bytes: {}",
                    MacAddressUtil.FormatAddress(bestMacAddr));
            }
            return bestMacAddr;
        }


        string NewLongValue()
        {
            var buf = StringBuilderManager.Allocate(2 * this.data.Length + 5);
            int i = 0;
            i = this.AppendHexDumpField(buf, i, MachineIdLen);
            i = this.AppendHexDumpField(buf, i, ProcessIdLen);
            i = this.AppendHexDumpField(buf, i, SequenceLen);
            i = this.AppendHexDumpField(buf, i, TimestampLen);
            i = this.AppendHexDumpField(buf, i, RandomLen);
            Debug.Assert(i == this.data.Length);
            var strValue = buf.ToString().Substring(0, buf.Length - 1); StringBuilderManager.Free(buf); return strValue;
        }

        int AppendHexDumpField(StringBuilder buf, int i, int length)
        {
            buf.Append(ByteBufferUtil.HexDump(this.data, i, length));
            buf.Append('-');
            i += length;
            return i;
        }

        void Init()
        {
            int i = 0;
            // machineId
            Array.Copy(MachineId, 0, this.data, i, MachineIdLen);
            i += MachineIdLen;

            // processId
            i = this.WriteInt(i, ProcessId);

            // sequence
            i = this.WriteInt(i, Interlocked.Increment(ref nextSequence));

            // timestamp (kind of)
            long ticks = Stopwatch.GetTimestamp();
            long nanos = (ticks / Stopwatch.Frequency) * 1000000000;
            long millis = (ticks / Stopwatch.Frequency) * 1000;
            i = this.WriteLong(i, ByteBufferUtil.SwapLong(nanos) ^ millis);

            // random
            int random = ThreadLocalRandom.Value.Next();
            this.hashCode = random;
            i = this.WriteInt(i, random);

            Debug.Assert(i == this.data.Length);
        }

        int WriteInt(int i, int value)
        {
            uint val = (uint)value;
            this.data[i++] = (byte)(val >> 24);
            this.data[i++] = (byte)(val >> 16);
            this.data[i++] = (byte)(val >> 8);
            this.data[i++] = (byte)value;
            return i;
        }

        int WriteLong(int i, long value)
        {
            ulong val = (ulong)value;
            this.data[i++] = (byte)(val >> 56);
            this.data[i++] = (byte)(val >> 48);
            this.data[i++] = (byte)(val >> 40);
            this.data[i++] = (byte)(val >> 32);
            this.data[i++] = (byte)(val >> 24);
            this.data[i++] = (byte)(val >> 16);
            this.data[i++] = (byte)(val >> 8);
            this.data[i++] = (byte)value;
            return i;
        }

        public override int GetHashCode() => this.hashCode;

        public override bool Equals(object obj)
        {
            return obj is DefaultChannelId channelId && this.Equals(channelId);
        }

        public bool Equals(IChannelId other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (other is DefaultChannelId channelId)
            {
                return this.hashCode == channelId.hashCode && Equals(this.data, channelId.data);
            }

            return false;
        }

        public override string ToString() => this.AsShortText();
    }
}
