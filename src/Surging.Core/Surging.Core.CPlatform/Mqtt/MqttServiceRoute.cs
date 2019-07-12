using Surging.Core.CPlatform.Address;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Surging.Core.CPlatform.Mqtt
{
    /// <summary>
    /// Defines the <see cref="MqttServiceRoute" />
    /// </summary>
    public class MqttServiceRoute
    {
        #region 属性

        /// <summary>
        /// Gets or sets the MqttDescriptor
        /// Mqtt服务描述符。
        /// </summary>
        public MqttDescriptor MqttDescriptor { get; set; }

        /// <summary>
        /// Gets or sets the MqttEndpoint
        /// Mqtt服务可用地址。
        /// </summary>
        public IEnumerable<AddressModel> MqttEndpoint { get; set; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The Equals
        /// </summary>
        /// <param name="obj">The object to compare with the current object. </param>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            var model = obj as MqttServiceRoute;
            if (model == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            if (model.MqttDescriptor != MqttDescriptor)
                return false;

            return model.MqttEndpoint.Count() == MqttEndpoint.Count() && model.MqttEndpoint.All(addressModel => MqttEndpoint.Contains(addressModel));
        }

        /// <summary>
        /// The GetHashCode
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        #endregion 方法

        public static bool operator ==(MqttServiceRoute model1, MqttServiceRoute model2)
        {
            return Equals(model1, model2);
        }

        public static bool operator !=(MqttServiceRoute model1, MqttServiceRoute model2)
        {
            return !Equals(model1, model2);
        }
    }
}