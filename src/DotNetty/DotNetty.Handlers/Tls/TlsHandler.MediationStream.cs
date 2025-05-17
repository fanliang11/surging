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

namespace DotNetty.Handlers.Tls
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Buffers;
    using DotNetty.Common.Utilities;

    partial class TlsHandler
    {
        private sealed partial class MediationStream : Stream
        {
            private readonly TlsHandler _owner;
            private CompositeByteBuffer _ownedInputBuffer;
            private int _inputOffset;
            private int _inputLength;
            private TaskCompletionSource<int> _readCompletionSource;

            public MediationStream(TlsHandler owner)
            {
                _owner = owner;
            }

            public int TotalReadableBytes
            {
                get
                {
                    var readableBytes = SourceReadableBytes;
                    if (_ownedInputBuffer is object)
                    {
                        readableBytes += _ownedInputBuffer.ReadableBytes;
                    }
                    return readableBytes;
                }
            }

            public int SourceReadableBytes => _inputLength - _inputOffset;

            public override void Flush()
            {
                // NOOP: called on SslStream.Close
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                if (disposing)
                {
                    TaskCompletionSource<int> p = _readCompletionSource;
                    if (p is object)
                    {
                        _readCompletionSource = null;
                        _ = p.TrySetResult(0);
                    }
                    _ownedInputBuffer.SafeRelease();
                    _ownedInputBuffer = null;
                }
            }

            #region plumbing

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return ReadAsync(buffer, offset, count).GetAwaiter().GetResult();
            }

            public override bool CanRead => true;

            public override bool CanSeek => false;

            public override bool CanWrite => true;

            public override long Length
            {
                get { throw new NotSupportedException(); }
            }

            public override long Position
            {
                get { throw new NotSupportedException(); }
                set { throw new NotSupportedException(); }
            }

            #endregion
        }
    }
}
