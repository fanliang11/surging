using Com.Ctrip.Framework.Apollo.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Configuration.Apollo.Configurations
{
    public static class ConfigFileFormatMethods
    {
        public static string GetString(this ConfigFileFormat format)
        {
            return format switch
            {
                ConfigFileFormat.Properties => "properties",
                ConfigFileFormat.Xml => "xml",
                ConfigFileFormat.Json => "json",
                ConfigFileFormat.Yml => "yml",
                ConfigFileFormat.Yaml => "yaml",
                ConfigFileFormat.Txt => "txt",
                _ => "unknown",
            };
        }
    }
}
