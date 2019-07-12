using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Configurations.Watch
{
    /// <summary>
    /// Defines the <see cref="ConfigurationWatch" />
    /// </summary>
    public abstract class ConfigurationWatch
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationWatch"/> class.
        /// </summary>
        protected ConfigurationWatch()
        {
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Process
        /// </summary>
        /// <returns>The <see cref="Task"/></returns>
        public abstract Task Process();

        #endregion 方法
    }
}