using Microsoft.Extensions.Configuration;

namespace Surging.Core.Consul.Configurations
{
    /// <summary>
    /// Defines the <see cref="ConsulConfigurationSource" />
    /// </summary>
    public class ConsulConfigurationSource : FileConfigurationSource
    {
        #region 属性

        /// <summary>
        /// Gets or sets the ConfigurationKeyPrefix
        /// </summary>
        public string ConfigurationKeyPrefix { get; set; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The Build
        /// </summary>
        /// <param name="builder">The builder<see cref="IConfigurationBuilder"/></param>
        /// <returns>The <see cref="IConfigurationProvider"/></returns>
        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            FileProvider = FileProvider ?? builder.GetFileProvider();
            return new ConsulConfigurationProvider(this);
        }

        #endregion 方法
    }
}