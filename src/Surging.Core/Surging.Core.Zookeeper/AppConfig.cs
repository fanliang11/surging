using Microsoft.Extensions.Configuration;

namespace Surging.Core.Zookeeper
{
    /// <summary>
    /// Defines the <see cref="AppConfig" />
    /// </summary>
    public class AppConfig
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Configuration
        /// </summary>
        public static IConfigurationRoot Configuration { get; set; }

        #endregion 属性
    }
}