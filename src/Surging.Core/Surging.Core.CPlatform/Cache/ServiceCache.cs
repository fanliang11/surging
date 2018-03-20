using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Surging.Core.CPlatform.Cache
{
    public class ServiceCache
    {
        /// <summary>
        /// 服务可用地址。
        /// </summary>
        public IEnumerable<CacheEndpoint> CacheEndpoint { get; set; }
        /// <summary>
        /// 服务描述符。
        /// </summary>
        public CacheDescriptor CacheDescriptor { get; set; }

        #region Equality members

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        /// <param name="obj">The object to compare with the current object. </param>
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

        /// <summary>Serves as the default hash function. </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public static bool operator ==(ServiceCache model1, ServiceCache model2)
        {
            return Equals(model1, model2);
        }

        public static bool operator !=(ServiceCache model1, ServiceCache model2)
        {
            return !Equals(model1, model2);
        }

        #endregion Equality members
    }
}
