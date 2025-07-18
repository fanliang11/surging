using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Tools.Cli.Internal.Implementation
{
    internal class GCAllModuleProvider : IGCModuleProvider
    {
        public void Collect()
        {
            GC.Collect();
        }
    }
}
