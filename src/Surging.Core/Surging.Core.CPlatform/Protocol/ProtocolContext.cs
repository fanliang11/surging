using Surging.Core.CPlatform.Module;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Protocol
{
    public class ProtocolContext
    {
        public ProtocolContext(
            string[] virtualPaths,
            CPlatformContainer serviceProvoider)
        { 
            VirtualPaths = Check.NotNull(virtualPaths, nameof(virtualPaths));
            ServiceProvoider = Check.NotNull(serviceProvoider, nameof(serviceProvoider));
        }
         

        public string[] VirtualPaths { get; }

        public CPlatformContainer ServiceProvoider { get; }
    }
}
 