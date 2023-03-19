using AutoMapper;
using AutoMapper.Attributes;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Utilities;
using System.Linq;

namespace Surging.Core.AutoMapper
{
    public class AutoMapperBootstrap : IAutoMapperBootstrap
    {
        public void Initialize()
        {
            var logger = ServiceLocator.GetService<ILogger<AutoMapperBootstrap>>();
            Mapper.Initialize(config => {
                if (AppConfig.Assemblies.Any())
                {
                    foreach (var assembly in AppConfig.Assemblies)
                    {
                        assembly.MapTypes(config);
                    }
                }

                var profiles = AppConfig.Profiles;
                if (profiles.Any())
                {
                    foreach (var profile in profiles)
                    {
                        logger.LogDebug($"解析到{profile.GetType().FullName}映射关系");
                        config.AddProfile(profile);
                    }
                }

            });
        }

    }
}
