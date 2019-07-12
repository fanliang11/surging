using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.WS.Runtime
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IWSServiceEntryProvider" />
    /// </summary>
    public interface IWSServiceEntryProvider
    {
        #region 方法

        /// <summary>
        /// The GetEntries
        /// </summary>
        /// <returns>The <see cref="IEnumerable{WSServiceEntry}"/></returns>
        IEnumerable<WSServiceEntry> GetEntries();

        #endregion 方法
    }

    #endregion 接口
}