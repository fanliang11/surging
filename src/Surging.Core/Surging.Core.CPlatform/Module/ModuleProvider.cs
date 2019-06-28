using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Engines;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Surging.Core.CPlatform.Module
{
    public class ModuleProvider: IModuleProvider
    {
        private readonly List<AbstractModule> _modules;
        private readonly string[] _virtualPaths;
        private readonly CPlatformContainer _serviceProvoider;
        private readonly ILogger<ModuleProvider> _logger;

        public ModuleProvider(List<AbstractModule> modules,
            string[] virtualPaths,
            ILogger<ModuleProvider> logger,
            CPlatformContainer serviceProvoider)
        {
            _modules = modules;
            _virtualPaths = virtualPaths;
            _serviceProvoider = serviceProvoider;
            _logger = logger;
        }

        public List<AbstractModule> Modules { get => _modules; }

        public string[] VirtualPaths { get => _virtualPaths; }

        public virtual void Initialize()
        {
            _modules.ForEach(p =>
            {
                try
                {
                    Type[] types = { typeof(SystemModule), typeof(BusinessModule), typeof(EnginePartModule), typeof(AbstractModule) }; 
                    if (p.Enable)
                            p.Initialize(new AppModuleContext(_modules, _virtualPaths, _serviceProvoider));
                    var type = p.GetType().BaseType;
                    if (types.Any(ty => ty == type))
                        p.Dispose();
                }
                catch(Exception ex)
                {
                    throw ex;
                }
            });
            WriteLog();
        }

        public void WriteLog()
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _modules.ForEach(p =>
                {
                    if (p.Enable)
                        _logger.LogDebug($"已初始化加载模块，类型：{p.TypeName}模块名：{p.ModuleName}。");
                });
            }
        }
    }
}
