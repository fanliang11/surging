using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Module;
using Surging.Core.KestrelHttpServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Volo.Abp.Configuration;
using VoloAbpModule = Volo.Abp.Modularity;
using Microsoft.Extensions.Configuration;
using Volo.Abp.DependencyInjection;
using  VoloAbp= Volo.Abp;


namespace Surging.Core.Abp
{
    [ExposeServices]
    public class AbpModule : KestrelHttpModule
    {
        private ILogger<AbpModule> _logger;
        private List<VoloAbp.IAbpApplicationWithExternalServiceProvider> _providers=new List<VoloAbp.IAbpApplicationWithExternalServiceProvider>();
        public override void Initialize(AppModuleContext context)
        {
            _logger = context.ServiceProvoider.GetInstances<ILogger<AbpModule>>();
        }

        public override void Initialize(ApplicationInitializationContext context)
        {
            _providers.ForEach(p => p.Initialize(context.Builder.ApplicationServices));
        }

        public override void Dispose()
        {
            base.Dispose();
            _providers = null;
        }

        public override void RegisterBuilder(ConfigurationContext context)
        {
            context.Services.AddSingleton<IConfigurationAccessor>(new DefaultConfigurationAccessor(context.Configuration));
            var referenceAssemblies = GetAssemblies(context.VirtualPaths).Concat(GetAssemblies());
            foreach (var moduleAssembly in referenceAssemblies)
            {
                GetAbstractModules(moduleAssembly).ForEach(p =>
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                        _logger.LogDebug($"已初始化加载Abp模块，类型：{p.GetType().FullName}模块名：{p.GetType().Name}。");
                    var application = VoloAbp.AbpApplicationFactory.Create(p.GetType(), context.Services);
                    _providers.Add(application);
                });
            }
        }
        /// <summary>
        /// Inject dependent third-party components
        /// </summary>
        /// <param name="builder"></param>
        protected override void RegisterBuilder(ContainerBuilderWrapper builder)
        {

        }

        private static List<VoloAbpModule.AbpModule> GetAbstractModules(Assembly assembly)
        {
            var abstractModules = new List<VoloAbpModule.AbpModule>();
            Type[] abpModule = assembly.GetTypes().Where(
            t => t.IsSubclassOf(typeof(VoloAbpModule.AbpModule))).ToArray();
            foreach (var moduleType in abpModule)
            {
                var abstractModule = (VoloAbpModule.AbpModule)Activator.CreateInstance(moduleType);
                abstractModules.Add(abstractModule);
            }
            return abstractModules;
        }

        private List<Assembly> GetAssemblies(params string[] virtualPaths)
        {
            var referenceAssemblies = new List<Assembly>();
            if (virtualPaths.Any())
            {
                referenceAssemblies = GetReferenceAssembly(virtualPaths);
            }
            else
            {
                string[] assemblyNames = DependencyContext
                    .Default.GetDefaultAssemblyNames().Select(p => p.Name).ToArray();
                assemblyNames = GetFilterAssemblies(assemblyNames);
                foreach (var name in assemblyNames)
                    referenceAssemblies.Add(Assembly.Load(name));
            }
            return referenceAssemblies;
        }

        private List<Assembly> GetReferenceAssembly(params string[] virtualPaths)
        {
            var refAssemblies = new List<Assembly>();
            var rootPath = AppContext.BaseDirectory;
            var existsPath = virtualPaths.Any();
            if (existsPath && !string.IsNullOrEmpty(AppConfig.ServerOptions.RootPath))
                rootPath = AppConfig.ServerOptions.RootPath;
            var paths = virtualPaths.Select(m => Path.Combine(rootPath, m)).ToList();
            if (!existsPath) paths.Add(rootPath);
            paths.ForEach(path =>
            {
                var assemblyFiles = GetAllAssemblyFiles(path);

                foreach (var referencedAssemblyFile in assemblyFiles)
                {
                    var referencedAssembly = Assembly.LoadFrom(referencedAssemblyFile);
                    refAssemblies.Add(referencedAssembly);
                }
            });
            return refAssemblies;
        }

        private List<string> GetAllAssemblyFiles(string parentDir)
        {
            var pattern = string.Format("^Volo.Abp.\\w*");
            Regex relatedRegex = new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return
                Directory.GetFiles(parentDir, "*.dll").Select(Path.GetFullPath).Where(
                    a => relatedRegex.IsMatch(a)).ToList();
        }

        private string[] GetFilterAssemblies(string[] assemblyNames)
        {
            var pattern = string.Format("^Volo.Abp.\\w*");
            Regex relatedRegex = new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return
                assemblyNames.Where(
                    name => relatedRegex.IsMatch(name)).ToArray();
        }
    }
}
