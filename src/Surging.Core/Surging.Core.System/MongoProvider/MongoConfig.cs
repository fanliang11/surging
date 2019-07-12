using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.System.MongoProvider
{
    /// <summary>
    /// Defines the <see cref="MongoConfig" />
    /// </summary>
    public class MongoConfig
    {
        #region 字段

        /// <summary>
        /// Defines the _config
        /// </summary>
        private readonly IConfigurationRoot _config;

        /// <summary>
        /// Defines the _configuration
        /// </summary>
        private static MongoConfig _configuration;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoConfig"/> class.
        /// </summary>
        /// <param name="Configuration">The Configuration<see cref="IConfigurationRoot"/></param>
        public MongoConfig(IConfigurationRoot Configuration)
        {
            _config = Configuration;
            _configuration = this;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the DefaultInstance
        /// </summary>
        public static MongoConfig DefaultInstance
        {
            get
            {
                return _configuration;
            }
        }

        /// <summary>
        /// Gets the MongConnectionString
        /// </summary>
        public string MongConnectionString
        {
            get
            {
                return _config["MongConnectionString"];
            }
        }

        #endregion 属性
    }
}