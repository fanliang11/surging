using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors.Implementation
{
    public enum AddressSelectorMode
    {
        HashAlgorithm,
        Polling,
        Random,
        FairPolling,
    }
}
