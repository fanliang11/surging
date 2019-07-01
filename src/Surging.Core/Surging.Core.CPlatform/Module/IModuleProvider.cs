using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Module
{
    public interface IModuleProvider
    {
        List<AbstractModule> Modules { get; }

        string[] VirtualPaths { get; }
        void Initialize();
    }
}
