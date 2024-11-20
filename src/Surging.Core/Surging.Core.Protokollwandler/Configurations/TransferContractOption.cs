using Surging.Core.Protokollwandler.Metadatas;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protokollwandler.Configurations
{
   public class TransferContractOption
    {
        public string Name { get; set; }

        public string RoutePath { get; set; }

        public string Endpoint { get; set; }

        public TransferContractType Type { get; set; }
    }
}
