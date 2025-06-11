using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core.Metadata
{
    public class Property : ConfigPropertyMetadata
    {
        public override string Code { get; }

        public override string Name { get; }

        public override string Description { get; }

        public override IDataType ValueType { get; }

        private ConfigVersion[] _versions;

        public override ConfigVersion[] GetConfigVersions()
        {
            return _versions == null ? new ConfigVersion[0] : _versions;
        }

        public Property(string code, string name, string description, IDataType valueType, params ConfigVersion[] versions)
        {
            Code = code;
            Name = name;
            Description = description;
            ValueType = valueType;
            _versions = versions;
        }
    }
}
