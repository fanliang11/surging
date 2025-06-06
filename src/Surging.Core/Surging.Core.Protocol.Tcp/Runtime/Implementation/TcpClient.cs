using DotNetty.Transport.Channels;
using Surging.Core.CPlatform.Codecs.Core;
using Surging.Core.CPlatform.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Tcp.Runtime.Implementation
{
    public class TcpClient : IClientConnection
    {
        private readonly IChannel  _channel;
        private bool _isAlive;
        private TimeSpan _keepAliveTimeout;

        public TcpClient(IChannel channel)
        {
            _channel = channel;
        }

        public void Connect()
        {
            _isAlive = true;
        }

        public async void Disconnect()
        {
            _isAlive = false;
            if (_channel.Active) 
             await _channel.CloseAsync();
        }

        public EndPoint GetClientAddress()
        {
            return _channel.RemoteAddress;
        }

        public string GetId()
        {
            return _channel.Id.AsLongText();
        }

        public TimeSpan GetKeepAliveTimeout()
        {
            return _keepAliveTimeout;
        }

        public bool IsAlive()
        {
            return _isAlive;
        }

        public IObservable<Task> OnConnect()
        {
           return Observable.Return(Task.Run(Connect));
        }

        public IObservable<Task> OnDisconnect()
        {
            return Observable.Return(_channel.CloseAsync());
        }

        public ISubject<IEncodedMessage> ReceiveMessage()
        {
            var subject = new AsyncSubject<IEncodedMessage>();
            return subject;
        }

        public IObservable<Task> SendMessage(IEncodedMessage message)
        {
            throw new NotImplementedException();
        }

        public void SetKeepAliveTimeout(TimeSpan timeout)
        {
          _keepAliveTimeout = timeout;
        }
    }
}
