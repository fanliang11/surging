using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.DNS.Runtime
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IDnsServiceEntryProvider" />
    /// </summary>
    public interface IDnsServiceEntryProvider
    {
        #region 方法

        /// <summary>
        /// The GetEntry
        /// </summary>
        /// <returns>The <see cref="DnsServiceEntry"/></returns>
        DnsServiceEntry GetEntry();

        #endregion 方法
    }

    #endregion 接口
}