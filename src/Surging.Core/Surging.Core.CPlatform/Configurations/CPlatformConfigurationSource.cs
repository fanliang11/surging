using Microsoft.Extensions.Configuration;

namespace Surging.Core.CPlatform.Configurations
{
    public class CPlatformConfigurationSource : FileConfigurationSource
    {
        public string ConfigurationKeyPrefix { get; set; }

        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            FileProvider = FileProvider ?? builder.GetFileProvider();
            return new CPlatformConfigurationProvider(this);
        }
    }
}