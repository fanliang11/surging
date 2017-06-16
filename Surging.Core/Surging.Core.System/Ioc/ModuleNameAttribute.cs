using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.System.Ioc
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ModuleNameAttribute : Attribute
    {
        public string ModuleName { get; set; }

        public ModuleNameAttribute()
        {

        }
        public ModuleNameAttribute(string moduleName)
        {
            ModuleName = moduleName;
        }
    }
}
