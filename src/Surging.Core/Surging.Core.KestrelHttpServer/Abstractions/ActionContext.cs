using Microsoft.AspNetCore.Http;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Runtime.Server;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.KestrelHttpServer
{
    public  class ActionContext
    {
        public ActionContext()
        {

        }

        public HttpContext HttpContext { get; set; }

        public TransportMessage Message { get; set; }
         
    }
}
