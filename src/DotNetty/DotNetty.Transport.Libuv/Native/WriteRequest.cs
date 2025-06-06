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

namespace DotNetty.Transport.Libuv.Native
{
    using System;
    using System.Buffers;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using DotNetty.Buffers;
    using DotNetty.Common;
    using DotNetty.Transport.Channels;

    sealed class WriteRequest : NativeRequest, ChannelOutboundBuffer.IMessageProcessor
    {
        private static readonly int BufferSize;
        private static readonly uv_watcher_cb WriteCallback = (h, s) => OnWriteCallback(h, s);

        private const int MaximumBytes = int.MaxValue;
        private const int MaximumLimit = 64;

        static WriteRequest()
        {
            BufferSize = Marshal.SizeOf<uv_buf_t>();
        }

        private readonly int _maxBytes;
        private readonly ThreadLocalPool.Handle _recyclerHandle;
        private readonly List<MemoryHandle> _handles;

        private IntPtr _bufs;
        private GCHandle _pin;
        private int _count;
        private int _size;

        private INativeUnsafe _nativeUnsafe;

        public WriteRequest(ThreadLocalPool.Handle recyclerHandle)
            : base(uv_req_type.UV_WRITE, BufferSize * MaximumLimit)
        {
            _recyclerHandle = recyclerHandle;

            int offset = NativeMethods.GetSize(uv_req_type.UV_WRITE);
            IntPtr addr = Handle;

            _maxBytes = MaximumBytes;
            _bufs = addr + offset;
            _pin = GCHandle.Alloc(addr, GCHandleType.Pinned);
            _handles = new List<MemoryHandle>(MaximumLimit + 1);
        }

        internal void DoWrite(INativeUnsafe channelUnsafe, ChannelOutboundBuffer input)
        {
            Debug.Assert(_nativeUnsafe is null);

            _nativeUnsafe = channelUnsafe;
            input.ForEachFlushedMessage(this);
            DoWrite();
        }

        bool Add(IByteBuffer buf)
        {
            if (_count == MaximumLimit) { return false; }

            int len = buf.ReadableBytes;
            if (0u >= (uint)len) { return true; }

            if (_maxBytes - len < _size && _count > 0) { return false; }

            if (buf.IsSingleIoBuffer)
            {
                var memory = buf.UnreadMemory;
                Add(memory.Pin(), memory.Length);
                return true;
            }

            return AddMany(buf);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        bool AddMany(IByteBuffer buf)
        {
            if (MaximumLimit - buf.IoBufferCount < _count) { return false; }

            var segments = buf.UnreadSequence;
            foreach (var memory in segments)
            {
                Add(memory.Pin(), memory.Length);
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe void Add(MemoryHandle memoryHandle, int len)
        {
            _handles.Add(memoryHandle);
            IntPtr baseOffset = MemoryAddress(_count);
            _size += len;
            ++_count;
            uv_buf_t.InitMemory(baseOffset, (IntPtr)memoryHandle.Pointer, len);
        }

        unsafe void DoWrite()
        {
            int result = NativeMethods.uv_write(
                Handle,
                _nativeUnsafe.UnsafeHandle,
                (uv_buf_t*)_bufs,
                _count,
                WriteCallback);

            if (result < 0)
            {
                Release();
                NativeMethods.ThrowOperationException((uv_err_code)result);
            }
        }

        public bool ProcessMessage(object msg) => msg is IByteBuffer buf && Add(buf);

        void Release()
        {
            var handleCount = _handles.Count;
            if (handleCount > 0)
            {
                for (int i = 0; i < handleCount; i++)
                {
                    _handles[i].Dispose();
                }
                _handles.Clear();
            }

            _nativeUnsafe = null;
            _count = 0;
            _size = 0;
            _recyclerHandle.Release(this);
        }

        void OnWriteCallback(int status)
        {
            INativeUnsafe @unsafe = _nativeUnsafe;
            int bytesWritten = _size;
            Release();

            OperationException error = null;
            if (status < 0)
            {
                error = NativeMethods.CreateError((uv_err_code)status);
            }
            @unsafe.FinishWrite(bytesWritten, error);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IntPtr MemoryAddress(int offset) => _bufs + BufferSize * offset;

        static void OnWriteCallback(IntPtr handle, int status)
        {
            var request = GetTarget<WriteRequest>(handle);
            request.OnWriteCallback(status);
        }

        void Free()
        {
            Release();
            if (_pin.IsAllocated)
            {
                _pin.Free();
            }
            _bufs = IntPtr.Zero;
        }

        protected override void Dispose(bool disposing)
        {
            if (_bufs != IntPtr.Zero)
            {
                Free();
            }
            base.Dispose(disposing);
        }
    }
}