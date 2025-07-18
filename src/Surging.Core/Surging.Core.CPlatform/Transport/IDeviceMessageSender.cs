using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Transport
{
    public interface IDeviceMessageSender
    {
        Task SendAndFlushAsync(object message);
    }
}
