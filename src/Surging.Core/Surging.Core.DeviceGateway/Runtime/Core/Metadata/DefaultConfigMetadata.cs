using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Formats.Asn1.AsnWriter;

namespace Surging.Core.DeviceGateway.Runtime.Core.Metadata
{
    public class DefaultConfigMetadata: ConfigMetadata
    {
        public string Name { get; private set; }

        public string Description { get; private set; }


        private ConfigVersion[] _versions= new ConfigVersion[0];

        public  List<ConfigPropertyMetadata> Properties { get; private set; }=new List<ConfigPropertyMetadata>();
        public DefaultConfigMetadata(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public override ConfigMetadata Copy(params ConfigVersion[] versions)
        {
            DefaultConfigMetadata configMetadata = new DefaultConfigMetadata(Name, Description);
            configMetadata.Version(_versions);
            if (_versions == null || _versions.Length == 0)
            {
                ConfigPropertyMetadata[] configProperties=new ConfigPropertyMetadata[0];
                Properties.CopyTo(configProperties); 
                configMetadata.Add(configProperties);
            }
            else
            {
                configMetadata.Add(
                        Properties.Where(p => p.HasAnyScope(versions))
                               .ToArray());
            } 
            return configMetadata;
        }

        public override ConfigVersion[] GetConfigVersions()
        {
            return  _versions;
        }

        public List<ConfigPropertyMetadata> GetProperties()
        { 
            return Properties; 
        }

        public DefaultConfigMetadata Add(ConfigPropertyMetadata metadata)
        {
            Properties.Add(metadata);
            return this;
        }

        public DefaultConfigMetadata Add(ConfigPropertyMetadata[] metadata)
        {
            Properties.AddRange(metadata);
            return this;
        }

        public DefaultConfigMetadata  Version(params ConfigVersion[] versions)
        {
            _versions = versions;
            return this;
        }

        public DefaultConfigMetadata Add(string code,
                                  string name,
                                  IDataType dataType,
                                  Action<Property> custom)
        {
            Property prop = new Property(code, name, Description, dataType, _versions);
            custom.Invoke(prop);
            return Add(prop);
        }

        public DefaultConfigMetadata Add(string code,
                                         string name,
                                         IDataType dataType)
        {
            return Add(code, name, null, dataType);
        }

        public DefaultConfigMetadata Add(string code,
                                   string name,
                                   string description,
                                   IDataType dataType)
        {
            return Add(new Property(code, name, description, dataType, _versions));
        }

        public DefaultConfigMetadata Add(string code,
                                         string name,
                                         IDataType dataType,
                                         params ConfigVersion[] versions)
        {
            return Add(code, name, null, dataType, versions);
        }

        public DefaultConfigMetadata Add(string code,
                                         string name,
                                         string description,
                                         IDataType dataType,
                                        params ConfigVersion[] versions)
        {
            return Add(new Property(code, name, description, dataType, versions));
        }
    }
}
