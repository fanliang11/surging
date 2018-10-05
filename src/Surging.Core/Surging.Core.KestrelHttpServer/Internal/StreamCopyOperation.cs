using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.KestrelHttpServer.Internal
{
   internal class StreamCopyOperation
    {
        private const int DefaultBufferSize = 1024 * 16;

        private readonly TaskCompletionSource<object> _tcs;
        private readonly Stream _source;
        private readonly Stream _destination;
        private readonly byte[] _buffer;
        private readonly AsyncCallback _readCallback;
        private readonly AsyncCallback _writeCallback;

        private long? _bytesRemaining;
        private CancellationToken _cancel;

        internal StreamCopyOperation(Stream source, Stream destination, long? bytesRemaining, CancellationToken cancel)
            : this(source, destination, bytesRemaining, DefaultBufferSize, cancel)
        {
        }

        internal StreamCopyOperation(Stream source, Stream destination, long? bytesRemaining, int bufferSize, CancellationToken cancel)
            : this(source, destination, bytesRemaining, new byte[bufferSize], cancel)
        {
        }

        internal StreamCopyOperation(Stream source, Stream destination, long? bytesRemaining, byte[] buffer, CancellationToken cancel)
        {
            Contract.Assert(source != null);
            Contract.Assert(destination != null);
            Contract.Assert(!bytesRemaining.HasValue || bytesRemaining.Value >= 0);
            Contract.Assert(buffer != null);

            _source = source;
            _destination = destination;
            _bytesRemaining = bytesRemaining;
            _cancel = cancel;
            _buffer = buffer;

            _tcs = new TaskCompletionSource<object>();
            _readCallback = new AsyncCallback(ReadCallback);
            _writeCallback = new AsyncCallback(WriteCallback);
        }

        public static async Task CopyToAsync(Stream source, Stream destination, long? count, int bufferSize, CancellationToken cancel)
        {
            long? bytesRemaining = count;

            var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
            try
            {
                Debug.Assert(source != null);
                Debug.Assert(destination != null);
                Debug.Assert(!bytesRemaining.HasValue || bytesRemaining.Value >= 0);
                Debug.Assert(buffer != null);

                while (true)
                {
                    if (bytesRemaining.HasValue && bytesRemaining.Value <= 0)
                    {
                        return;
                    }

                    cancel.ThrowIfCancellationRequested();

                    int readLength = buffer.Length;
                    if (bytesRemaining.HasValue)
                    {
                        readLength = (int)Math.Min(bytesRemaining.Value, (long)readLength);
                    }
                    int read = await source.ReadAsync(buffer, 0, readLength, cancel);

                    if (bytesRemaining.HasValue)
                    {
                        bytesRemaining -= read;
                    }
                    
                    if (read <= 0)
                    {
                        return;
                    }

                    cancel.ThrowIfCancellationRequested();

                    destination.Write(buffer, 0, read);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        internal Task Start()
        {
            ReadNextSegment();
            return _tcs.Task;
        }

        private void Complete()
        {
            _tcs.TrySetResult(null);
        }

        private bool CheckCancelled()
        {
            if (_cancel.IsCancellationRequested)
            {
                _tcs.TrySetCanceled();
                return true;
            }
            return false;
        }

        private void Fail(Exception ex)
        {
            _tcs.TrySetException(ex);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Redirecting")]
        private void ReadNextSegment()
        {
            // The natural end of the range.
            if (_bytesRemaining.HasValue && _bytesRemaining.Value <= 0)
            {
                Complete();
                return;
            }

            if (CheckCancelled())
            {
                return;
            }

            try
            {
                int readLength = _buffer.Length;
                if (_bytesRemaining.HasValue)
                {
                    readLength = (int)Math.Min(_bytesRemaining.Value, (long)readLength);
                }
                IAsyncResult async = _source.BeginRead(_buffer, 0, readLength, _readCallback, null);

                if (async.CompletedSynchronously)
                {
                    int read = _source.EndRead(async);
                    WriteToOutputStream(read);
                }
            }
            catch (Exception ex)
            {
                Fail(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Redirecting")]
        private void ReadCallback(IAsyncResult async)
        {
            if (async.CompletedSynchronously)
            {
                return;
            }

            try
            {
                int read = _source.EndRead(async);
                WriteToOutputStream(read);
            }
            catch (Exception ex)
            {
                Fail(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Redirecting")]
        private void WriteToOutputStream(int count)
        {
            if (_bytesRemaining.HasValue)
            {
                _bytesRemaining -= count;
            }
             
            if (count == 0)
            {
                Complete();
                return;
            }

            if (CheckCancelled())
            {
                return;
            }

            try
            {
                IAsyncResult async = _destination.BeginWrite(_buffer, 0, count, _writeCallback, null);
                if (async.CompletedSynchronously)
                {
                    _destination.EndWrite(async);
                    ReadNextSegment();
                }
            }
            catch (Exception ex)
            {
                Fail(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Redirecting")]
        private void WriteCallback(IAsyncResult async)
        {
            if (async.CompletedSynchronously)
            {
                return;
            }

            try
            {
                _destination.EndWrite(async);
                ReadNextSegment();
            }
            catch (Exception ex)
            {
                Fail(ex);
            }
        }
    }
}
