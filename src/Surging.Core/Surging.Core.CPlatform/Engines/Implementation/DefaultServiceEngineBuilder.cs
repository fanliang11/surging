using Autofac;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace Surging.Core.CPlatform.Engines.Implementation
{
    public class DefaultServiceEngineBuilder : IServiceEngineBuilder
    {
        private readonly VirtualPathProviderServiceEngine _serviceEngine;
        public DefaultServiceEngineBuilder(IServiceEngine serviceEngine)
        {
            _serviceEngine = serviceEngine as VirtualPathProviderServiceEngine;
        }

        public void Build(ContainerBuilder serviceContainer)
        {
            var serviceBuilder = new ServiceBuilder(serviceContainer);
            var virtualPaths = new List<string>();
            if (_serviceEngine != null)
            {
                if (_serviceEngine.ModuleServiceLocationFormats != null)
                {
                    var paths = GetPaths(_serviceEngine.ModuleServiceLocationFormats);
                    if (paths == null) return;
                    serviceBuilder.RegisterServices(paths);
                    serviceBuilder.RegisterRepositories(paths);
                }
                if (_serviceEngine.ComponentServiceLocationFormats != null)
                {
                    var paths = GetPaths(_serviceEngine.ComponentServiceLocationFormats);
                    if (paths == null) return;
                    serviceBuilder.RegisterModules(paths);
                }
            }
        }

        private string [] GetPaths(params string [] virtualPaths)
        {
            var Directories = new List<string>(virtualPaths.Where(p=>!string.IsNullOrEmpty(p))) ;
            string rootPath =string.IsNullOrEmpty(AppConfig.ServerOptions.RootPath)? 
                AppContext.BaseDirectory: AppConfig.ServerOptions.RootPath;
            var virPaths = virtualPaths; 
            foreach (var virtualPath in virtualPaths)
            {
                var path = Path.Combine(rootPath, virtualPath);
                if (Directory.Exists(path))
                {
                    var dirs = Directory.GetDirectories(path);
                    Directories.AddRange(dirs.Select(dir => Path.Combine(virtualPath, new DirectoryInfo(dir).Name)));
                }
                else
                {
                    Directories.Remove(virtualPath);
                    virPaths = null;
                }
            }
            return Directories.Any() ?Directories.Distinct().ToArray(): virPaths;
        }
    }
}
