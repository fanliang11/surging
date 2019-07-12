using Surging.Core.CPlatform.Address;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Mqtt.Internal.Runtime
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IMqttBrokerEntryManger" />
    /// </summary>
    public interface IMqttBrokerEntryManger
    {
        #region 方法

        /// <summary>
        /// The CancellationReg
        /// </summary>
        /// <param name="topic">The topic<see cref="string"/></param>
        /// <param name="addressModel">The addressModel<see cref="AddressModel"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task CancellationReg(string topic, AddressModel addressModel);

        /// <summary>
        /// The GetMqttBrokerAddress
        /// </summary>
        /// <param name="topic">The topic<see cref="string"/></param>
        /// <returns>The <see cref="ValueTask{IEnumerable{AddressModel}}"/></returns>
        ValueTask<IEnumerable<AddressModel>> GetMqttBrokerAddress(string topic);

        /// <summary>
        /// The Register
        /// </summary>
        /// <param name="topic">The topic<see cref="string"/></param>
        /// <param name="addressModel">The addressModel<see cref="AddressModel"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task Register(string topic, AddressModel addressModel);

        #endregion 方法
    }

    #endregion 接口
}