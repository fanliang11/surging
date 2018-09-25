using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.System.MongoProvider
{
    public class MongoConfig
    {
        private static MongoConfig _configuration;
        private readonly IConfigurationRoot _config;

        public MongoConfig(IConfigurationRoot Configuration)
        {
            _config = Configuration;
            _configuration = this;
        }

        public static MongoConfig DefaultInstance
        {
            get
            {
                return _configuration;
            }
        }
        public string MongConnectionString
        {
            get
            {
                return _config["MongConnectionString"];
            }
        }

    }
}
