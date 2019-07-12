using Surging.Core.Protocol.Mqtt.Internal.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt.Internal.Runtime
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IMqttBehaviorProvider" />
    /// </summary>
    public interface IMqttBehaviorProvider
    {
        #region 方法

        /// <summary>
        /// The GetMqttBehavior
        /// </summary>
        /// <returns>The <see cref="MqttBehavior"/></returns>
        MqttBehavior GetMqttBehavior();

        #endregion 方法
    }

    #endregion 接口
}