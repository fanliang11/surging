using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Module
{
    public class ModuleProvider: IModuleProvider
    {
        private readonly List<AbstractModule> _modules;
        public ModuleProvider(List<AbstractModule> modules)
        {
            _modules = modules;
        }

        public void Initialize()
        {
            _modules.ForEach(p => p.Initialize());
        }
    }
}
