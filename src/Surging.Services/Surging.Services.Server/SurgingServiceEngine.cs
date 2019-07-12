using Surging.Core.CPlatform.Engines.Implementation;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Surging.Services.Server
{
    /// <summary>
    /// Defines the <see cref="SurgingServiceEngine" />
    /// </summary>
    public class SurgingServiceEngine : VirtualPathProviderServiceEngine
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="SurgingServiceEngine"/> class.
        /// </summary>
        public SurgingServiceEngine()
        {
            ModuleServiceLocationFormats = new[] {
                EnvironmentHelper.GetEnvironmentVariable("${ModulePath1}|Modules"),
            };
            ComponentServiceLocationFormats = new[] {
                 EnvironmentHelper.GetEnvironmentVariable("${ComponentPath1}|Components"),
            };
        }

        #endregion 构造函数
    }
}