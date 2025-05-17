using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Runtime.Client
{
    public interface IServiceMonitorWatcher
    {
        ValueTask AddNodeMonitorWatcher(string serviceId);
    }
}
