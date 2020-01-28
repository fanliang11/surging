using Com.Ctrip.Framework.Apollo;
using Com.Ctrip.Framework.Apollo.Enums;
using Com.Ctrip.Framework.Apollo.Spi;
using System;
using System.Linq;

namespace Surging.Core.Configuration.Apollo.Configurations
{
    public static class ApolloConfigurationBuilderExtensions
    {

        /// <summary>添加其他namespace</summary>
        public static IApolloConfigurationBuilder AddNamespaceSurgingApollo(this IApolloConfigurationBuilder builder, string @namespace, ConfigFileFormat format = ConfigFileFormat.Json) =>
            builder.AddNamespaceSurgingApollo(@namespace, null, format);

        /// <summary>添加其他namespace。如果sectionKey为null则添加到root中，可以直接读取，否则使用Configuration.GetSection(sectionKey)读取</summary>
        public static IApolloConfigurationBuilder AddNamespaceSurgingApollo(this IApolloConfigurationBuilder builder, string @namespace, string? sectionKey, ConfigFileFormat format = ConfigFileFormat.Json)
        {
            if (string.IsNullOrWhiteSpace(@namespace)) throw new ArgumentNullException(nameof(@namespace));
            if (format < ConfigFileFormat.Properties || format > ConfigFileFormat.Txt) throw new ArgumentOutOfRangeException(nameof(format), format, $"最小值{ConfigFileFormat.Properties}，最大值{ConfigFileFormat.Txt}");

            if (format != ConfigFileFormat.Properties) @namespace += "." + format.GetString();

            var configRepository = builder.ConfigRepositoryFactory.GetConfigRepository(@namespace);
            var previous = builder.Sources.FirstOrDefault(source =>
                source is SurgingApolloConfigurationProvider apollo &&
                apollo.SectionKey == sectionKey &&
                apollo.ConfigRepository == configRepository);
            if (previous != null)
            {
                builder.Sources.Remove(previous);
                builder.Sources.Add(previous);
            }
            else
            {
                builder.Add(new SurgingApolloConfigurationProvider(sectionKey, configRepository));
#pragma warning disable 618
                ApolloConfigurationManager.Manager.Registry.Register(@namespace, new DefaultConfigFactory(builder.ConfigRepositoryFactory));
#pragma warning restore 618
            }

            return builder;
        }
    }
}
