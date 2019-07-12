using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Filters
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IFilter" />
    /// </summary>
    public interface IFilter
    {
        #region 属性

        /// <summary>
        /// Gets a value indicating whether AllowMultiple
        /// </summary>
        bool AllowMultiple { get; }

        #endregion 属性
    }

    #endregion 接口
}