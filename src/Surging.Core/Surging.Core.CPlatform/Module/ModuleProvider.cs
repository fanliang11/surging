using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Engines;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Surging.Core.CPlatform.Module
{
    /// <summary>
    /// Defines the <see cref="ModuleProvider" />
    /// </summary>
    public class ModuleProvider : IModuleProvider
    {
        #region 字段

        /// <summary>
        /// Defines the _logger
        /// </summary>
        private readonly ILogger<ModuleProvider> _logger;

        /// <summary>
        /// Defines the _modules
        /// </summary>
        private readonly List<AbstractModule> _modules;

        /// <summary>
        /// Defines the _serviceProvoider
        /// </summary>
        private readonly CPlatformContainer _serviceProvoider;

        /// <summary>
        /// Defines the _virtualPaths
        /// </summary>
        private readonly string[] _virtualPaths;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleProvider"/> class.
        /// </summary>
        /// <param name="modules"></param>
        /// <param name="virtualPaths">The virtualPaths<see cref="string[]"/></param>
        /// <param name="logger"></param>
        /// <param name="serviceProvoider"></param>
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

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the Modules
        /// </summary>
        public List<AbstractModule> Modules { get => _modules; }

        /// <summary>
        /// Gets the VirtualPaths
        /// </summary>
        public string[] VirtualPaths { get => _virtualPaths; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The Initialize
        /// </summary>
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
                catch (Exception ex)
                {
                    throw ex;
                }
            });
            WriteLog();
        }

        /// <summary>
        /// The WriteLog
        /// </summary>
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

        #endregion 方法
    }
}