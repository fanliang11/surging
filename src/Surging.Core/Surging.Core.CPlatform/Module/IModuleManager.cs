using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Surging.Core.CPlatform.Module
{
   public  interface IModuleManager
    {
        bool Install(string modulePackageFileName, TextWriter textWriter);

        void Start(AssemblyEntry module);
        
        void Stop(AssemblyEntry module);
        
        void Uninstall(AssemblyEntry module);

        void Delete(AssemblyEntry module);
        
        void Save(AssemblyEntry module);
    }
}
