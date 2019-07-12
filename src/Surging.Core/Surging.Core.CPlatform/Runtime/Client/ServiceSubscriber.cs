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
        #region 属性

        /// <summary>
        /// Gets or sets the Address
        /// 订阅者服务地址。
        /// </summary>
        public IEnumerable<AddressModel> Address { get; set; }

        /// <summary>
        /// Gets or sets the ServiceDescriptor
        /// 服务描述符。
        /// </summary>
        public ServiceDescriptor ServiceDescriptor { get; set; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The Equals
        /// </summary>
        /// <param name="obj">The obj<see cref="object"/></param>
        /// <returns>The <see cref="bool"/></returns>
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

        /// <summary>
        /// The GetHashCode
        /// </summary>
        /// <returns>The <see cref="int"/></returns>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        #endregion 方法

        public static bool operator ==(ServiceSubscriber model1, ServiceSubscriber model2)
        {
            return Equals(model1, model2);
        }

        public static bool operator !=(ServiceSubscriber model1, ServiceSubscriber model2)
        {
            return !Equals(model1, model2);
        }
    }
}