using Surging.Core.CPlatform.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protokollwandler.Internal
{
   public interface IMessageSender
    { 

        Task SendAndFlushAsync(string message, string contentType);
    }
}
