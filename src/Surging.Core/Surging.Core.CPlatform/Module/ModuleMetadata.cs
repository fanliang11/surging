using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Module
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ModuleMetadata : Attribute
    {
        public string Title { get; set; }
        public string Description { get; set; }

        public bool Enable { get; set; } 
    }
}
