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
    /// <summary>
    /// Defines the <see cref="StreamCopyOperation" />
    /// </summary>
    internal class StreamCopyOperation
    {
        #region 常量

        /// <summary>
        /// Defines the DefaultBufferSize
        /// </summary>
        private const int DefaultBufferSize = 1024 * 16;

        #endregion 常量

        #region 字段

        /// <summary>
        /// Defines the _buffer
        /// </summary>
        private readonly byte[] _buffer;

        /// <summary>
        /// Defines the _destination
        /// </summary>
        private readonly Stream _destination;

        /// <summary>
        /// Defines the _readCallback
        /// </summary>
        private readonly AsyncCallback _readCallback;

        /// <summary>
        /// Defines the _source
        /// </summary>
        private readonly Stream _source;

        /// <summary>
        /// Defines the _tcs
        /// </summary>
        private readonly TaskCompletionSource<object> _tcs;

        /// <summary>
        /// Defines the _writeCallback
        /// </summary>
        private readonly AsyncCallback _writeCallback;

        /// <summary>
        /// Defines the _bytesRemaining
        /// </summary>
        private long? _bytesRemaining;

        /// <summary>
        /// Defines the _cancel
        /// </summary>
        private CancellationToken _cancel;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamCopyOperation"/> class.
        /// </summary>
        /// <param name="source">The source<see cref="Stream"/></param>
        /// <param name="destination">The destination<see cref="Stream"/></param>
        /// <param name="bytesRemaining">The bytesRemaining<see cref="long?"/></param>
        /// <param name="buffer">The buffer<see cref="byte[]"/></param>
        /// <param name="cancel">The cancel<see cref="CancellationToken"/></param>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamCopyOperation"/> class.
        /// </summary>
        /// <param name="source">The source<see cref="Stream"/></param>
        /// <param name="destination">The destination<see cref="Stream"/></param>
        /// <param name="bytesRemaining">The bytesRemaining<see cref="long?"/></param>
        /// <param name="cancel">The cancel<see cref="CancellationToken"/></param>
        internal StreamCopyOperation(Stream source, Stream destination, long? bytesRemaining, CancellationToken cancel)
            : this(source, destination, bytesRemaining, DefaultBufferSize, cancel)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamCopyOperation"/> class.
        /// </summary>
        /// <param name="source">The source<see cref="Stream"/></param>
        /// <param name="destination">The destination<see cref="Stream"/></param>
        /// <param name="bytesRemaining">The bytesRemaining<see cref="long?"/></param>
        /// <param name="bufferSize">The bufferSize<see cref="int"/></param>
        /// <param name="cancel">The cancel<see cref="CancellationToken"/></param>
        internal StreamCopyOperation(Stream source, Stream destination, long? bytesRemaining, int bufferSize, CancellationToken cancel)
            : this(source, destination, bytesRemaining, new byte[bufferSize], cancel)
        {
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The CopyToAsync
        /// </summary>
        /// <param name="source">The source<see cref="Stream"/></param>
        /// <param name="destination">The destination<see cref="Stream"/></param>
        /// <param name="count">The count<see cref="long?"/></param>
        /// <param name="bufferSize">The bufferSize<see cref="int"/></param>
        /// <param name="cancel">The cancel<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task"/></returns>
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

        /// <summary>
        /// The Start
        /// </summary>
        /// <returns>The <see cref="Task"/></returns>
        internal Task Start()
        {
            ReadNextSegment();
            return _tcs.Task;
        }

        /// <summary>
        /// The CheckCancelled
        /// </summary>
        /// <returns>The <see cref="bool"/></returns>
        private bool CheckCancelled()
        {
            if (_cancel.IsCancellationRequested)
            {
                _tcs.TrySetCanceled();
                return true;
            }
            return false;
        }

        /// <summary>
        /// The Complete
        /// </summary>
        private void Complete()
        {
            _tcs.TrySetResult(null);
        }

        /// <summary>
        /// The Fail
        /// </summary>
        /// <param name="ex">The ex<see cref="Exception"/></param>
        private void Fail(Exception ex)
        {
            _tcs.TrySetException(ex);
        }

        /// <summary>
        /// The ReadCallback
        /// </summary>
        /// <param name="async">The async<see cref="IAsyncResult"/></param>
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

        /// <summary>
        /// The ReadNextSegment
        /// </summary>
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

        /// <summary>
        /// The WriteCallback
        /// </summary>
        /// <param name="async">The async<see cref="IAsyncResult"/></param>
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

        /// <summary>
        /// The WriteToOutputStream
        /// </summary>
        /// <param name="count">The count<see cref="int"/></param>
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

        #endregion 方法
    }
}