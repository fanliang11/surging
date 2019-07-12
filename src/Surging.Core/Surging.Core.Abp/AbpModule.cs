using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
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
using Volo.Abp.DependencyInjection;
using VoloAbp = Volo.Abp;
using VoloAbpModule = Volo.Abp.Modularity;

namespace Surging.Core.Abp
{
    /// <summary>
    /// Defines the <see cref="AbpModule" />
    /// </summary>
    [ExposeServices]
    public class AbpModule : KestrelHttpModule
    {
        #region 字段

        /// <summary>
        /// Defines the _logger
        /// </summary>
        private ILogger<AbpModule> _logger;

        /// <summary>
        /// Defines the _providers
        /// </summary>
        private List<VoloAbp.IAbpApplicationWithExternalServiceProvider> _providers = new List<VoloAbp.IAbpApplicationWithExternalServiceProvider>();

        #endregion 字段

        #region 方法

        /// <summary>
        /// The Dispose
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            _providers = null;
        }

        /// <summary>
        /// The Initialize
        /// </summary>
        /// <param name="context">The context<see cref="ApplicationInitializationContext"/></param>
        public override void Initialize(ApplicationInitializationContext context)
        {
            _providers.ForEach(p => p.Initialize(context.Builder.ApplicationServices));
        }

        /// <summary>
        /// The Initialize
        /// </summary>
        /// <param name="context">The context<see cref="AppModuleContext"/></param>
        public override void Initialize(AppModuleContext context)
        {
            _logger = context.ServiceProvoider.GetInstances<ILogger<AbpModule>>();
        }

        /// <summary>
        /// The RegisterBuilder
        /// </summary>
        /// <param name="context">The context<see cref="ConfigurationContext"/></param>
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

        /// <summary>
        /// The GetAbstractModules
        /// </summary>
        /// <param name="assembly">The assembly<see cref="Assembly"/></param>
        /// <returns>The <see cref="List{VoloAbpModule.AbpModule}"/></returns>
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

        /// <summary>
        /// The GetAllAssemblyFiles
        /// </summary>
        /// <param name="parentDir">The parentDir<see cref="string"/></param>
        /// <returns>The <see cref="List{string}"/></returns>
        private List<string> GetAllAssemblyFiles(string parentDir)
        {
            var pattern = string.Format("^Volo.Abp.\\w*");
            Regex relatedRegex = new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return
                Directory.GetFiles(parentDir, "*.dll").Select(Path.GetFullPath).Where(
                    a => relatedRegex.IsMatch(a)).ToList();
        }

        /// <summary>
        /// The GetAssemblies
        /// </summary>
        /// <param name="virtualPaths">The virtualPaths<see cref="string[]"/></param>
        /// <returns>The <see cref="List{Assembly}"/></returns>
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

        /// <summary>
        /// The GetFilterAssemblies
        /// </summary>
        /// <param name="assemblyNames">The assemblyNames<see cref="string[]"/></param>
        /// <returns>The <see cref="string[]"/></returns>
        private string[] GetFilterAssemblies(string[] assemblyNames)
        {
            var pattern = string.Format("^Volo.Abp.\\w*");
            Regex relatedRegex = new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return
                assemblyNames.Where(
                    name => relatedRegex.IsMatch(name)).ToArray();
        }

        /// <summary>
        /// The GetReferenceAssembly
        /// </summary>
        /// <param name="virtualPaths">The virtualPaths<see cref="string[]"/></param>
        /// <returns>The <see cref="List{Assembly}"/></returns>
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

        #endregion 方法
    }
}