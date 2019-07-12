using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.EventBusRabbitMQ
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IConsumeConfigurator" />
    /// </summary>
    public interface IConsumeConfigurator
    {
        #region 方法

        /// <summary>
        /// The Configure
        /// </summary>
        /// <param name="consumers">The consumers<see cref="List{Type}"/></param>
        void Configure(List<Type> consumers);

        /// <summary>
        /// The Unconfigure
        /// </summary>
        /// <param name="consumers">The consumers<see cref="List{Type}"/></param>
        void Unconfigure(List<Type> consumers);

        #endregion 方法
    }

    #endregion 接口
}