using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Transport
{
    public interface IHttpMessageSender: IMessageSender
    {
        Task SendAndFlushAsync(string payload, Dictionary<string, string> headers);
    }
}
