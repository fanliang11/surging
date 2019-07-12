using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.EventBus
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="ISubscriptionAdapt" />
    /// </summary>
    public interface ISubscriptionAdapt
    {
        #region 方法

        /// <summary>
        /// The SubscribeAt
        /// </summary>
        void SubscribeAt();

        /// <summary>
        /// The Unsubscribe
        /// </summary>
        void Unsubscribe();

        #endregion 方法
    }

    #endregion 接口
}