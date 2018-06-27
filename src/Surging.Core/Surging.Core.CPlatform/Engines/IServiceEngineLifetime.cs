using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Surging.Core.CPlatform.Engines
{
   public interface IServiceEngineLifetime
    {
        CancellationToken ServiceEngineStarted { get; }

        CancellationToken ServiceEngineStopping { get; }

        CancellationToken ServiceEngineStopped { get; }


        void StopApplication();

        void NotifyStopped();

        void NotifyStarted();
    }
}
