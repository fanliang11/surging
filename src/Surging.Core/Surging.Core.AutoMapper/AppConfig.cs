using AutoMapper;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Engines;
using Surging.Core.CPlatform.Engines.Implementation;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CPlatformAppConfig = Surging.Core.CPlatform.AppConfig;

namespace Surging.Core.AutoMapper
{
    public class AppConfig
    {

        public static IEnumerable<Assembly> Assemblies
        {
            get
            {
                var assemblies = new List<Assembly>();
                var referenceAssemblies = GetAllReferenceAssemblies();
                foreach (var assembiyString in AssembliesStrings)
                {
                    assemblies.AddRange(referenceAssemblies.Where(p => p.FullName == assembiyString || Regex.IsMatch(p.FullName, assembiyString)));
                }
                return assemblies;
            }
        }

        public static IEnumerable<Profile> Profiles
        {
            get
            {
                var logger = ServiceLocator.GetService<ILogger<AppConfig>>();
                var profiles = new List<Profile>();
                var referenceAssemblies = GetAllReferenceAssemblies();
                foreach (var assembly in referenceAssemblies)
                {
                    var profileTypes = assembly.DefinedTypes.Select(p => p.AsType()).Where(p => typeof(Profile).IsAssignableFrom(p) && !p.IsAbstract).ToList();
                    if (profileTypes.Any())
                    {
                        foreach (var profileType in profileTypes)
                        {
                            try
                            {
                                var profile = Activator.CreateInstance(profileType) as Profile;
                                profiles.Add(profile);
                            }
                            catch (Exception e)
                            {
                                if (logger.IsEnabled(LogLevel.Warning))
                                    logger.LogWarning($"构建profile失败,profile类型为{profileType.FullName}");
                            }

                        }
                    }
                }
                return profiles;
            }
        }

        private static IEnumerable<Assembly> GetAllReferenceAssemblies()
        {
            var serviceEngine = ServiceLocator.GetService<IServiceEngine>() as VirtualPathProviderServiceEngine;
            string[] paths = null;
            if (serviceEngine != null)
            {
                if (serviceEngine.ModuleServiceLocationFormats != null)
                {
                    paths = GetPaths(serviceEngine.ModuleServiceLocationFormats);
                }
            }
            var referenceAssemblies = paths == null ? GetReferenceAssembly() : GetReferenceAssembly(paths);
            return referenceAssemblies;
        }

        private static List<Assembly> _referenceAssembly = new List<Assembly>();

        public static IEnumerable<string> AssembliesStrings { get; internal set; }

        private static List<Assembly> GetReferenceAssembly(params string[] virtualPaths)
        {
            var refAssemblies = new List<Assembly>();//Assembly 通过此类能够载入操纵一个程序集，并获取程序集内部信息
            var rootPath = AppContext.BaseDirectory;
            var existsPath = virtualPaths.Any();//判断是否有数据
            if (existsPath && !string.IsNullOrEmpty(CPlatformAppConfig.ServerOptions.RootPath))
                rootPath = CPlatformAppConfig.ServerOptions.RootPath;
            var result = _referenceAssembly;
            if (!result.Any() || existsPath)
            {
                var paths = virtualPaths.Select(m => Path.Combine(rootPath, m)).ToList();
                if (!existsPath) paths.Add(rootPath);
                paths.ForEach(path =>
                {
                    var assemblyFiles = GetAllAssemblyFiles(path);

                    foreach (var referencedAssemblyFile in assemblyFiles)
                    {
                        var referencedAssembly = Assembly.LoadFrom(referencedAssemblyFile);
                        if (!_referenceAssembly.Contains(referencedAssembly))
                            _referenceAssembly.Add(referencedAssembly);
                        refAssemblies.Add(referencedAssembly);
                    }
                    result = existsPath ? refAssemblies : _referenceAssembly;
                });
            }
            return result;
        }

        private static List<string> GetAllAssemblyFiles(string parentDir)
        {
            var notRelatedFile = CPlatformAppConfig.ServerOptions.NotRelatedAssemblyFiles;
            var relatedFile = CPlatformAppConfig.ServerOptions.RelatedAssemblyFiles;
            var pattern = string.Format("^Microsoft.\\w*|^System.\\w*|^Netty.\\w*|^Autofac.\\w*{0}",
               string.IsNullOrEmpty(notRelatedFile) ? "" : $"|{notRelatedFile}");
            Regex notRelatedRegex = new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Regex relatedRegex = new Regex(relatedFile, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            if (!string.IsNullOrEmpty(relatedFile))
            {
                return
                    Directory.GetFiles(parentDir, "*.dll").Select(Path.GetFullPath).Where(
                        a => !notRelatedRegex.IsMatch(a) && relatedRegex.IsMatch(a)).ToList();
            }
            else
            {
                return
                    Directory.GetFiles(parentDir, "*.dll").Select(Path.GetFullPath).Where(
                        a => !notRelatedRegex.IsMatch(a)).ToList();
            }
        }

        private static string[] GetPaths(params string[] virtualPaths)
        {
            var directories = new List<string>(virtualPaths.Where(p => !string.IsNullOrEmpty(p)));
            string rootPath = string.IsNullOrEmpty(CPlatformAppConfig.ServerOptions.RootPath) ?
                AppContext.BaseDirectory : CPlatformAppConfig.ServerOptions.RootPath;
            var virPaths = virtualPaths;
            foreach (var virtualPath in virtualPaths)
            {
                var path = Path.Combine(rootPath, virtualPath);
                if (Directory.Exists(path))
                {
                    var dirs = Directory.GetDirectories(path);
                    directories.AddRange(dirs.Select(dir => Path.Combine(virtualPath, new DirectoryInfo(dir).Name)));
                }
                else
                {
                    directories.Remove(virtualPath);
                    virPaths = null;
                }
            }
            return directories.Any() ? directories.Distinct().ToArray() : virPaths;
        }

    }
}
