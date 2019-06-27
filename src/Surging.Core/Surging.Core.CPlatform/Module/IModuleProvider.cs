using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Module
{
    public interface IModuleProvider
    {
         List<AbstractModule> Modules { get;}
        void Initialize();
    }
}
