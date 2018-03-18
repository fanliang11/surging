using Surging.Core.CPlatform.Configurations.Watch;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Configurations
{
    public  interface IConfigurationWatchManager
    {
        void Register(ConfigurationWatch watch);
    }
}
