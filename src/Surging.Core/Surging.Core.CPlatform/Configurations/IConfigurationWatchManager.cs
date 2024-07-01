using Surging.Core.CPlatform.Configurations.Watch;

namespace Surging.Core.CPlatform.Configurations
{
    public  interface IConfigurationWatchManager
    {
        void Register(ConfigurationWatch watch);
    }
}
