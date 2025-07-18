using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Surging.Core.CPlatform.Codecs.Core;

namespace Surging.Core.CPlatform.Protocol
{
    public interface IClientConnection
    {
        string GetId();
        EndPoint GetClientAddress();

        IObservable<Task> SendMessage(IEncodedMessage message);

        ISubject<IEncodedMessage> ReceiveMessage();

        void Connect();

        IObservable<Task> OnConnect();

        void Disconnect();
         

        bool IsAlive();

        TimeSpan GetKeepAliveTimeout();

        void SetKeepAliveTimeout(TimeSpan timeout);

        IObservable<Task> OnDisconnect();
    }
}
