using Surging.Core.CPlatform.Address;
using System.Collections.Generic;
using System.Linq;

namespace Surging.Core.CPlatform.Routing
{
    /// <summary>
    /// 服务路由。
    /// </summary>
    public class ServiceRoute
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Address
        /// 服务可用地址。
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
        /// <param name="obj">The object to compare with the current object. </param>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            var model = obj as ServiceRoute;
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
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        #endregion 方法

        public static bool operator ==(ServiceRoute model1, ServiceRoute model2)
        {
            return Equals(model1, model2);
        }

        public static bool operator !=(ServiceRoute model1, ServiceRoute model2)
        {
            return !Equals(model1, model2);
        }
    }
}