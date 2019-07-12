using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Caching.Configurations
{
    /// <summary>
    /// Defines the <see cref="CacheConfigurationSource" />
    /// </summary>
    public class CacheConfigurationSource : FileConfigurationSource
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
            return new CacheConfigurationProvider(this);
        }

        #endregion 方法
    }
}