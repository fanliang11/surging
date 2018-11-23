using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Surging.Core.Protocol.Mqtt.Internal.Runtime
{
    public abstract  class Runnable 
    {
       private volatile Thread _runnableThread;
        public Runnable()
        {
            var watcherThread = new Thread(s => ((Runnable)s).Run());
            watcherThread.Start(this);
            _runnableThread = watcherThread;
        }

        public abstract void Run();
         
    }
}
