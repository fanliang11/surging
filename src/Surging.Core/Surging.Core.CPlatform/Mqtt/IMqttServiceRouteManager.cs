using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Mqtt.Implementation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Mqtt
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IMqttServiceRouteManager" />
    /// </summary>
    public interface IMqttServiceRouteManager
    {
        #region 事件

        /// <summary>
        /// Defines the Changed
        /// </summary>
        event EventHandler<MqttServiceRouteChangedEventArgs> Changed;

        /// <summary>
        /// Defines the Created
        /// </summary>
        event EventHandler<MqttServiceRouteEventArgs> Created;

        /// <summary>
        /// Defines the Removed
        /// </summary>
        event EventHandler<MqttServiceRouteEventArgs> Removed;

        #endregion 事件

        #region 方法

        /// <summary>
        /// 清空所有的服务路由。
        /// </summary>
        /// <returns>一个任务。</returns>
        Task ClearAsync();

        /// <summary>
        /// The GetRoutesAsync
        /// </summary>
        /// <returns>The <see cref="Task{IEnumerable{MqttServiceRoute}}"/></returns>
        Task<IEnumerable<MqttServiceRoute>> GetRoutesAsync();

        /// <summary>
        /// The RemoveByTopicAsync
        /// </summary>
        /// <param name="topic">The topic<see cref="string"/></param>
        /// <param name="endpoint">The endpoint<see cref="IEnumerable{AddressModel}"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task RemoveByTopicAsync(string topic, IEnumerable<AddressModel> endpoint);

        /// <summary>
        /// The RemveAddressAsync
        /// </summary>
        /// <param name="addresses">The addresses<see cref="IEnumerable{AddressModel}"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task RemveAddressAsync(IEnumerable<AddressModel> addresses);

        /// <summary>
        /// 设置服务路由。
        /// </summary>
        /// <param name="routes">服务路由集合。</param>
        /// <returns>一个任务。</returns>
        Task SetRoutesAsync(IEnumerable<MqttServiceRoute> routes);

        #endregion 方法
    }

    #endregion 接口
}