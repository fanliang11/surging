using Surging.Core.KestrelHttpServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Stage.Internal
{
    public interface IWebServerListener
    {
        void Listen(WebHostContext context);
    }
}
