using Surging.Core.CPlatform.Address;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Surging.Core.CPlatform.Runtime.Client
{
    /// <summary>
    /// 服务订阅者
    /// </summary>
    public class ServiceSubscriber
    {
        /// <summary>
        /// 订阅者服务地址。
        /// </summary>
        public IEnumerable<AddressModel> Address { get; set; }

        /// <summary>
        /// 服务描述符。
        /// </summary>
        public ServiceDescriptor ServiceDescriptor { get; set; }

        #region Equality members

        public override bool Equals(object obj)
        {
            var model = obj as ServiceSubscriber;
            if (model == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            if (model.ServiceDescriptor != ServiceDescriptor)
                return false;

            return model.Address.Count() == Address.Count() && model.Address.All(addressModel => Address.Contains(addressModel));
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public static bool operator ==(ServiceSubscriber model1, ServiceSubscriber model2)
        {
            return Equals(model1, model2);
        }

        public static bool operator !=(ServiceSubscriber model1, ServiceSubscriber model2)
        {
            return !Equals(model1, model2);
        }

        #endregion Equality members
    }
}