using Microsoft.Extensions.Configuration;

namespace Surging.Core.Configuration.Apollo.Configurations
{
    public interface IConfigurationFactory
    {
        IConfiguration Create();
    }
}