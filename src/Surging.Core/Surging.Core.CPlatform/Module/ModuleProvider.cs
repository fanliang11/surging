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
        }

        public void Initialize()
        {
            _modules.ForEach(p => p.Initialize(_serviceProvoider));
        }
    }
}
