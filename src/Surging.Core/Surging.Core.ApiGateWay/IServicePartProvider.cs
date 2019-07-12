using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.ApiGateWay
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IServicePartProvider" />
    /// </summary>
    public interface IServicePartProvider
    {
        #region 方法

        /// <summary>
        /// The IsPart
        /// </summary>
        /// <param name="routhPath">The routhPath<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        bool IsPart(string routhPath);

        /// <summary>
        /// The Merge
        /// </summary>
        /// <param name="routhPath">The routhPath<see cref="string"/></param>
        /// <param name="param">The param<see cref="Dictionary{string, object}"/></param>
        /// <returns>The <see cref="Task{object}"/></returns>
        Task<object> Merge(string routhPath, Dictionary<string, object> param);

        #endregion 方法
    }

    #endregion 接口
}