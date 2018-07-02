using Surging.Core.CPlatform.Engines;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Module
{
    public class ModuleProvider: IModuleProvider
    {
        private readonly List<AbstractModule> _modules;
        private readonly CPlatformContainer _serviceProvoider;
        public ModuleProvider(List<AbstractModule> modules,CPlatformContainer serviceProvoider)
        {
            _modules = modules;
            _serviceProvoider = serviceProvoider;
        }

        public void Initialize()
        {
            _serviceProvoider.GetInstances<IServiceEngineLifetime>().ServiceEngineStarted.Register(() =>
            {
                _modules.ForEach(p =>
                {
                    if (p.Enable)
                        p.Initialize(_serviceProvoider);
                });
            });
        }
    }
}
