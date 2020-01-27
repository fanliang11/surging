﻿using Com.Ctrip.Framework.Apollo;
using Com.Ctrip.Framework.Apollo.Enums;
using Microsoft.Extensions.Configuration;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Surging.Core.CPlatform.Configurations.Apollo
{
    public static class ApolloConfigurationExtensions
    {


        public static IConfigurationBuilder UseApollo(this IConfigurationBuilder builder, Action<IApolloConfigurationBuilder> action, string sectionName="apollo", string path= "${apollopath}|apollo.json")
        {
            Check.NotNull(builder, "builder");

            var section = AppConfig.GetSection(sectionName);

            if (!section.Exists())
            {
                if (!string.IsNullOrEmpty(AppConfig.ServerOptions.RootPath))
                {
                    var apolloPath = Path.Combine(AppConfig.ServerOptions.RootPath, "apollo.json");

                    builder.AddCPlatformFile(apolloPath, optional: false, reloadOnChange: true);
                }

                //builder.AddCPlatformFile("${apollopath}|apollo.json", optional: false, reloadOnChange: true);
                builder.AddCPlatformFile(path, optional: false, reloadOnChange: true);
            }

            var config = builder.Build();
            section = config.GetSection("apollo");
            if (!section.Exists())
            {
                throw new Exception("apollo config file not exists!");
            }

            var apollo = builder.AddApollo(section);
            if (action == null)
            {
                apollo.AddNamespaceSurgingApollo("surgingSettings");
            }
            else
            {
                action(apollo);
            }


            AppConfig.Configuration = builder.Build();
            AppConfig.ServerOptions = AppConfig.Configuration.Get<SurgingServerOptions>();
            var surgingSection = AppConfig.Configuration.GetSection("Surging");
            if (surgingSection.Exists())
                AppConfig.ServerOptions = AppConfig.Configuration.GetSection("Surging").Get<SurgingServerOptions>();

            return builder;
        }
    }
}