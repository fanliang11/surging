/*
 * HttpConnection.cs
 *
 * This code is derived from HttpConnection.cs (System.Net) of Mono
 * (http://www.mono-project.com).
 *
 * The MIT License
 *
 * Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
 * Copyright (c) 2012-2016 sta.blockhead
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

/*
 * Authors:
 * - Gonzalo Paniagua Javier <gonzalo@novell.com>
 */

/*
 * Contributors:
 * - Liryna <liryna.stark@gmail.com>
 * - Rohan Singh <rohan-singh@hotmail.com>
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace WebSocketCore.Net
{
    /// <summary>
    /// Defines the <see cref="HttpConnection" />
    /// </summary>
    internal sealed class HttpConnection
    {
        #region 常量

        /// <summary>
        /// Defines the _bufferLength
        /// </summary>
        private const int _bufferLength = 8192;

        #endregion 常量

        #region 字段

        /// <summary>
        /// Defines the _buffer
        /// </summary>
        private byte[] _buffer;

        /// <summary>
        /// Defines the _context
        /// </summary>
        private HttpListenerContext _context;

        /// <summary>
        /// Defines the _contextRegistered
        /// </summary>
        private bool _contextRegistered;

        /// <summary>
        /// Defines the _currentLine
        /// </summary>
        private StringBuilder _currentLine;

        /// <summary>
        /// Defines the _inputState
        /// </summary>
        private InputState _inputState;

        /// <summary>
        /// Defines the _inputStream
        /// </summary>
        private RequestStream _inputStream;

        /// <summary>
        /// Defines the _lastListener
        /// </summary>
        private HttpListener _lastListener;

        /// <summary>
        /// Defines the _lineState
        /// </summary>
        private LineState _lineState;

        /// <summary>
        /// Defines the _listener
        /// </summary>
        private EndPointListener _listener;

        /// <summary>
        /// Defines the _localEndPoint
        /// </summary>
        private EndPoint _localEndPoint;

        /// <summary>
        /// Defines the _outputStream
        /// </summary>
        private ResponseStream _outputStream;

        /// <summary>
        /// Defines the _position
        /// </summary>
        private int _position;

        /// <summary>
        /// Defines the _remoteEndPoint
        /// </summary>
        private EndPoint _remoteEndPoint;

        /// <summary>
        /// Defines the _requestBuffer
        /// </summary>
        private MemoryStream _requestBuffer;

        /// <summary>
        /// Defines the _reuses
        /// </summary>
        private int _reuses;

        /// <summary>
        /// Defines the _secure
        /// </summary>
        private bool _secure;

        /// <summary>
        /// Defines the _socket
        /// </summary>
        private Socket _socket;

        /// <summary>
        /// Defines the _stream
        /// </summary>
        private Stream _stream;

        /// <summary>
        /// Defines the _sync
        /// </summary>
        private object _sync;

        /// <summary>
        /// Defines the _timeout
        /// </summary>
        private int _timeout;

        /// <summary>
        /// Defines the _timeoutCanceled
        /// </summary>
        private Dictionary<int, bool> _timeoutCanceled;

        /// <summary>
        /// Defines the _timer
        /// </summary>
        private Timer _timer;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpConnection"/> class.
        /// </summary>
        /// <param name="socket">The socket<see cref="Socket"/></param>
        /// <param name="listener">The listener<see cref="EndPointListener"/></param>
        internal HttpConnection(Socket socket, EndPointListener listener)
        {
            _socket = socket;
            _listener = listener;

            var netStream = new NetworkStream(socket, false);
            if (listener.IsSecure)
            {
                var sslConf = listener.SslConfiguration;
                var sslStream = new SslStream(
                                  netStream,
                                  false,
                                  sslConf.ClientCertificateValidationCallback
                                );

                sslStream.AuthenticateAsServer(
                  sslConf.ServerCertificate,
                  sslConf.ClientCertificateRequired,
                  sslConf.EnabledSslProtocols,
                  sslConf.CheckCertificateRevocation
                );

                _secure = true;
                _stream = sslStream;
            }
            else
            {
                _stream = netStream;
            }

            _localEndPoint = socket.LocalEndPoint;
            _remoteEndPoint = socket.RemoteEndPoint;
            _sync = new object();
            _timeout = 90000; // 90k ms for first request, 15k ms from then on.
            _timeoutCanceled = new Dictionary<int, bool>();
            _timer = new Timer(onTimeout, this, Timeout.Infinite, Timeout.Infinite);

            init();
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets a value indicating whether IsClosed
        /// </summary>
        public bool IsClosed
        {
            get
            {
                return _socket == null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether IsLocal
        /// </summary>
        public bool IsLocal
        {
            get
            {
                return ((IPEndPoint)_remoteEndPoint).Address.IsLocal();
            }
        }

        /// <summary>
        /// Gets a value indicating whether IsSecure
        /// </summary>
        public bool IsSecure
        {
            get
            {
                return _secure;
            }
        }

        /// <summary>
        /// Gets the LocalEndPoint
        /// </summary>
        public IPEndPoint LocalEndPoint
        {
            get
            {
                return (IPEndPoint)_localEndPoint;
            }
        }

        /// <summary>
        /// Gets the RemoteEndPoint
        /// </summary>
        public IPEndPoint RemoteEndPoint
        {
            get
            {
                return (IPEndPoint)_remoteEndPoint;
            }
        }

        /// <summary>
        /// Gets the Reuses
        /// </summary>
        public int Reuses
        {
            get
            {
                return _reuses;
            }
        }

        /// <summary>
        /// Gets the Stream
        /// </summary>
        public Stream Stream
        {
            get
            {
                return _stream;
            }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The BeginReadRequest
        /// </summary>
        public void BeginReadRequest()
        {
            if (_buffer == null)
                _buffer = new byte[_bufferLength];

            if (_reuses == 1)
                _timeout = 15000;

            try
            {
                _timeoutCanceled.Add(_reuses, false);
                _timer.Change(_timeout, Timeout.Infinite);
                _stream.BeginRead(_buffer, 0, _bufferLength, onRead, this);
            }
            catch
            {
                close();
            }
        }

        /// <summary>
        /// The Close
        /// </summary>
        public void Close()
        {
            Close(false);
        }

        /// <summary>
        /// The GetRequestStream
        /// </summary>
        /// <param name="contentLength">The contentLength<see cref="long"/></param>
        /// <param name="chunked">The chunked<see cref="bool"/></param>
        /// <returns>The <see cref="RequestStream"/></returns>
        public RequestStream GetRequestStream(long contentLength, bool chunked)
        {
            lock (_sync)
            {
                if (_socket == null)
                    return null;

                if (_inputStream != null)
                    return _inputStream;

                var buff = _requestBuffer.GetBuffer();
                var len = (int)_requestBuffer.Length;
                var cnt = len - _position;
                disposeRequestBuffer();

                _inputStream = chunked
                               ? new ChunkedRequestStream(
                                   _stream, buff, _position, cnt, _context
                                 )
                               : new RequestStream(
                                   _stream, buff, _position, cnt, contentLength
                                 );

                return _inputStream;
            }
        }

        /// <summary>
        /// The GetResponseStream
        /// </summary>
        /// <returns>The <see cref="ResponseStream"/></returns>
        public ResponseStream GetResponseStream()
        {
            // TODO: Can we get this stream before reading the input?

            lock (_sync)
            {
                if (_socket == null)
                    return null;

                if (_outputStream != null)
                    return _outputStream;

                var lsnr = _context.Listener;
                var ignore = lsnr != null ? lsnr.IgnoreWriteExceptions : true;
                _outputStream = new ResponseStream(_stream, _context.Response, ignore);

                return _outputStream;
            }
        }

        /// <summary>
        /// The SendError
        /// </summary>
        public void SendError()
        {
            SendError(_context.ErrorMessage, _context.ErrorStatus);
        }

        /// <summary>
        /// The SendError
        /// </summary>
        /// <param name="message">The message<see cref="string"/></param>
        /// <param name="status">The status<see cref="int"/></param>
        public void SendError(string message, int status)
        {
            if (_socket == null)
                return;

            lock (_sync)
            {
                if (_socket == null)
                    return;

                try
                {
                    var res = _context.Response;
                    res.StatusCode = status;
                    res.ContentType = "text/html";

                    var content = new StringBuilder(64);
                    content.AppendFormat("<html><body><h1>{0} {1}", status, res.StatusDescription);
                    if (message != null && message.Length > 0)
                        content.AppendFormat(" ({0})</h1></body></html>", message);
                    else
                        content.Append("</h1></body></html>");

                    var enc = Encoding.UTF8;
                    var entity = enc.GetBytes(content.ToString());
                    res.ContentEncoding = enc;
                    res.ContentLength64 = entity.LongLength;

                    res.Close(entity, true);
                }
                catch
                {
                    Close(true);
                }
            }
        }

        /// <summary>
        /// The Close
        /// </summary>
        /// <param name="force">The force<see cref="bool"/></param>
        internal void Close(bool force)
        {
            if (_socket == null)
                return;

            lock (_sync)
            {
                if (_socket == null)
                    return;

                if (force)
                {
                    if (_outputStream != null)
                        _outputStream.Close(true);

                    close();
                    return;
                }

                GetResponseStream().Close(false);

                if (_context.Response.CloseConnection)
                {
                    close();
                    return;
                }

                if (!_context.Request.FlushInput())
                {
                    close();
                    return;
                }

                disposeRequestBuffer();
                unregisterContext();
                init();

                _reuses++;
                BeginReadRequest();
            }
        }

        /// <summary>
        /// The onRead
        /// </summary>
        /// <param name="asyncResult">The asyncResult<see cref="IAsyncResult"/></param>
        private static void onRead(IAsyncResult asyncResult)
        {
            var conn = (HttpConnection)asyncResult.AsyncState;
            if (conn._socket == null)
                return;

            lock (conn._sync)
            {
                if (conn._socket == null)
                    return;

                var nread = -1;
                var len = 0;
                try
                {
                    var current = conn._reuses;
                    if (!conn._timeoutCanceled[current])
                    {
                        conn._timer.Change(Timeout.Infinite, Timeout.Infinite);
                        conn._timeoutCanceled[current] = true;
                    }

                    nread = conn._stream.EndRead(asyncResult);
                    conn._requestBuffer.Write(conn._buffer, 0, nread);
                    len = (int)conn._requestBuffer.Length;
                }
                catch (Exception ex)
                {
                    if (conn._requestBuffer != null && conn._requestBuffer.Length > 0)
                    {
                        conn.SendError(ex.Message, 400);
                        return;
                    }

                    conn.close();
                    return;
                }

                if (nread <= 0)
                {
                    conn.close();
                    return;
                }

                if (conn.processInput(conn._requestBuffer.GetBuffer(), len))
                {
                    if (!conn._context.HasError)
                        conn._context.Request.FinishInitialization();

                    if (conn._context.HasError)
                    {
                        conn.SendError();
                        return;
                    }

                    HttpListener lsnr;
                    if (!conn._listener.TrySearchHttpListener(conn._context.Request.Url, out lsnr))
                    {
                        conn.SendError(null, 404);
                        return;
                    }

                    if (conn._lastListener != lsnr)
                    {
                        conn.removeConnection();
                        if (!lsnr.AddConnection(conn))
                        {
                            conn.close();
                            return;
                        }

                        conn._lastListener = lsnr;
                    }

                    conn._context.Listener = lsnr;
                    if (!conn._context.Authenticate())
                        return;

                    if (conn._context.Register())
                        conn._contextRegistered = true;

                    return;
                }

                conn._stream.BeginRead(conn._buffer, 0, _bufferLength, onRead, conn);
            }
        }

        /// <summary>
        /// The onTimeout
        /// </summary>
        /// <param name="state">The state<see cref="object"/></param>
        private static void onTimeout(object state)
        {
            var conn = (HttpConnection)state;
            var current = conn._reuses;
            if (conn._socket == null)
                return;

            lock (conn._sync)
            {
                if (conn._socket == null)
                    return;

                if (conn._timeoutCanceled[current])
                    return;

                conn.SendError(null, 408);
            }
        }

        /// <summary>
        /// The close
        /// </summary>
        private void close()
        {
            lock (_sync)
            {
                if (_socket == null)
                    return;

                disposeTimer();
                disposeRequestBuffer();
                disposeStream();
                closeSocket();
            }

            unregisterContext();
            removeConnection();
        }

        /// <summary>
        /// The closeSocket
        /// </summary>
        private void closeSocket()
        {
            try
            {
                _socket.Shutdown(SocketShutdown.Both);
            }
            catch
            {
            }

            _socket.Close();
            _socket = null;
        }

        /// <summary>
        /// The disposeRequestBuffer
        /// </summary>
        private void disposeRequestBuffer()
        {
            if (_requestBuffer == null)
                return;

            _requestBuffer.Dispose();
            _requestBuffer = null;
        }

        /// <summary>
        /// The disposeStream
        /// </summary>
        private void disposeStream()
        {
            if (_stream == null)
                return;

            _inputStream = null;
            _outputStream = null;

            _stream.Dispose();
            _stream = null;
        }

        /// <summary>
        /// The disposeTimer
        /// </summary>
        private void disposeTimer()
        {
            if (_timer == null)
                return;

            try
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
            catch
            {
            }

            _timer.Dispose();
            _timer = null;
        }

        /// <summary>
        /// The init
        /// </summary>
        private void init()
        {
            _context = new HttpListenerContext(this);
            _inputState = InputState.RequestLine;
            _inputStream = null;
            _lineState = LineState.None;
            _outputStream = null;
            _position = 0;
            _requestBuffer = new MemoryStream();
        }

        // true -> Done processing.
        // false -> Need more input.
        /// <summary>
        /// The processInput
        /// </summary>
        /// <param name="data">The data<see cref="byte[]"/></param>
        /// <param name="length">The length<see cref="int"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool processInput(byte[] data, int length)
        {
            if (_currentLine == null)
                _currentLine = new StringBuilder(64);

            var nread = 0;
            try
            {
                string line;
                while ((line = readLineFrom(data, _position, length, out nread)) != null)
                {
                    _position += nread;
                    if (line.Length == 0)
                    {
                        if (_inputState == InputState.RequestLine)
                            continue;

                        if (_position > 32768)
                            _context.ErrorMessage = "Headers too long";

                        _currentLine = null;
                        return true;
                    }

                    if (_inputState == InputState.RequestLine)
                    {
                        _context.Request.SetRequestLine(line);
                        _inputState = InputState.Headers;
                    }
                    else
                    {
                        _context.Request.AddHeader(line);
                    }

                    if (_context.HasError)
                        return true;
                }
            }
            catch (Exception ex)
            {
                _context.ErrorMessage = ex.Message;
                return true;
            }

            _position += nread;
            if (_position >= 32768)
            {
                _context.ErrorMessage = "Headers too long";
                return true;
            }

            return false;
        }

        /// <summary>
        /// The readLineFrom
        /// </summary>
        /// <param name="buffer">The buffer<see cref="byte[]"/></param>
        /// <param name="offset">The offset<see cref="int"/></param>
        /// <param name="length">The length<see cref="int"/></param>
        /// <param name="read">The read<see cref="int"/></param>
        /// <returns>The <see cref="string"/></returns>
        private string readLineFrom(byte[] buffer, int offset, int length, out int read)
        {
            read = 0;

            for (var i = offset; i < length && _lineState != LineState.Lf; i++)
            {
                read++;

                var b = buffer[i];
                if (b == 13)
                    _lineState = LineState.Cr;
                else if (b == 10)
                    _lineState = LineState.Lf;
                else
                    _currentLine.Append((char)b);
            }

            if (_lineState != LineState.Lf)
                return null;

            var line = _currentLine.ToString();

            _currentLine.Length = 0;
            _lineState = LineState.None;

            return line;
        }

        /// <summary>
        /// The removeConnection
        /// </summary>
        private void removeConnection()
        {
            if (_lastListener != null)
                _lastListener.RemoveConnection(this);
            else
                _listener.RemoveConnection(this);
        }

        /// <summary>
        /// The unregisterContext
        /// </summary>
        private void unregisterContext()
        {
            if (!_contextRegistered)
                return;

            _context.Unregister();
            _contextRegistered = false;
        }

        #endregion 方法
    }
}