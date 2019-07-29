using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Stage.Configurations
{
    public class AccessSettingOption
    {
        public string WhiteList { get; set; }

        public string BlackList { get; set; }

        public string RoutePath { get; set; }

        public bool Enable { get; set; }
    }
}
