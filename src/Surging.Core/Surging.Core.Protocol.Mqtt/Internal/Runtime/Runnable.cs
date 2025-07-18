using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Surging.Core.Protocol.Mqtt.Internal.Runtime
{
    public abstract  class Runnable 
    {
       private volatile Thread _runnableThread;
        private readonly Timer _timer;
        public Runnable()
        {
            var timeSpan = TimeSpan.FromSeconds(3);
            _timer = new Timer(s =>
           {
               Run();
           }, null, timeSpan, timeSpan);
        }

        public abstract void Run();
         
    }
}
