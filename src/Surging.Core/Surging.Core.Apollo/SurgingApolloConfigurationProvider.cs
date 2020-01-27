﻿using Com.Ctrip.Framework.Apollo;
using Com.Ctrip.Framework.Apollo.Core.Utils;
using Com.Ctrip.Framework.Apollo.Internals;
using Microsoft.Extensions.Configuration;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.Apollo
{
    public class SurgingApolloConfigurationProvider : ApolloConfigurationProvider
    {
        internal string SectionKey { get; }
        internal IConfigRepository ConfigRepository { get; }

        public SurgingApolloConfigurationProvider(string sectionKey, IConfigRepository configRepository) : base(sectionKey, configRepository)
        {
            SectionKey = sectionKey;
            ConfigRepository = configRepository;
        }

        protected override void SetData(Properties properties)
        {
            var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var key in properties.GetPropertyNames())
            {
                if (string.IsNullOrEmpty(SectionKey))
                    data[key] = EnvironmentHelper.GetEnvironmentVariable(properties.GetProperty(key) ?? string.Empty);
                else
                    data[$"{SectionKey}{ConfigurationPath.KeyDelimiter}{key}"] = EnvironmentHelper.GetEnvironmentVariable(properties.GetProperty(key) ?? string.Empty);
            }

            Data = data;
        }

    }
}