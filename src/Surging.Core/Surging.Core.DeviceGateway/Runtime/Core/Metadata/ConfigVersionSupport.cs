using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core.Metadata
{
    public abstract class ConfigVersionSupport
    {
        private readonly ConfigVersion[] configVersions = new ConfigVersion[0];

        public abstract ConfigVersion[] GetConfigVersions();

        public bool HasAnyScope(params ConfigVersion[] target)
        {
            if (target.Length == 0 || GetConfigVersions() == configVersions)
            {
                return true;
            }
            return target
                    .Any(p => HasAnyScope(p));
        }

        public bool HasScope(ConfigVersion target)
        {
            if (GetConfigVersions() == configVersions)
            {
                return true;
            }
            foreach (ConfigVersion version in GetConfigVersions())
            {
                if (version.VersionNum.Equals(target.VersionNum))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
