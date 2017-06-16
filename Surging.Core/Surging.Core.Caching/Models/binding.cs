using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Caching.Models
{
    public  class Binding
    {
        public string Id { get; set; }

        public string Class { get; set; }

        public string InitMethod { get; set; }

        public List<Map> Maps { get; set; }

        public List<Property> Properties { get; set; }
    }
}
