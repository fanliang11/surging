using Surging.Core.CPlatform.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Tools.Cli.Internal
{
  public  interface ITransportClient
    {
        Task<RemoteInvokeResultMessage> SendAsync(CancellationToken cancellationToken);
    }
}
