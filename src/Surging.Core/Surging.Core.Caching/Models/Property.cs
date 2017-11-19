using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Caching.Models
{
    public class Property
    {
        public string Name { get; set; }

        public string Ref { get; set; }

        public string Value { get; set; }

        public List<Map> Maps { get; set; }
    }
}
