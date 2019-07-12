using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Surging.Core.CPlatform.Cache
{
    /// <summary>
    /// Defines the <see cref="ServiceCache" />
    /// </summary>
    public class ServiceCache
    {
        #region 属性

        /// <summary>
        /// Gets or sets the CacheDescriptor
        /// 服务描述符。
        /// </summary>
        public CacheDescriptor CacheDescriptor { get; set; }

        /// <summary>
        /// Gets or sets the CacheEndpoint
        /// 服务可用地址。
        /// </summary>
        public IEnumerable<CacheEndpoint> CacheEndpoint { get; set; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The Equals
        /// </summary>
        /// <param name="obj">The object to compare with the current object. </param>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            var model = obj as ServiceCache;
            if (model == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            if (model.CacheDescriptor != CacheDescriptor)
                return false;

            return model.CacheEndpoint.Count() == CacheEndpoint.Count() && model.CacheEndpoint.All(addressModel => CacheEndpoint.Contains(addressModel));
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

        public static bool operator ==(ServiceCache model1, ServiceCache model2)
        {
            return Equals(model1, model2);
        }

        public static bool operator !=(ServiceCache model1, ServiceCache model2)
        {
            return !Equals(model1, model2);
        }
    }
}