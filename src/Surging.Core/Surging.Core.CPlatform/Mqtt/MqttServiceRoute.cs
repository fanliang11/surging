using Surging.Core.CPlatform.Address;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Surging.Core.CPlatform.Mqtt
{
   public  class MqttServiceRoute
    {
        /// <summary>
        /// Mqtt服务可用地址。
        /// </summary>
        public IEnumerable<AddressModel> MqttEndpoint { get; set; }
        /// <summary>
        /// Mqtt服务描述符。
        /// </summary>
        public MqttDescriptor MqttDescriptor { get; set; }

        #region Equality members

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        /// <param name="obj">The object to compare with the current object. </param>
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

        /// <summary>Serves as the default hash function. </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public static bool operator ==(MqttServiceRoute model1, MqttServiceRoute model2)
        {
            return Equals(model1, model2);
        }

        public static bool operator != (MqttServiceRoute model1, MqttServiceRoute model2)
        {
            return !Equals(model1, model2);
        }

        #endregion Equality members
    }
}
