using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.ServiceHosting.Extensions.Runtime
{
   public interface IBackgroundServiceEntryProvider
    {
        IEnumerable<BackgroundServiceEntry> GetEntries(); 
    }
}
