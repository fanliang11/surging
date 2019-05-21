using Newtonsoft.Json;
using System;
using System.Net;

namespace Surging.Core.CPlatform.Address
{
    /// <summary>
    /// 一个抽象的地址模型。
    /// </summary>
    public abstract class AddressModel
    {
        /// <summary>
        /// 创建终结点。
        /// </summary>
        /// <returns></returns>
        public abstract EndPoint CreateEndPoint();

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public decimal ProcessorTime { get; set; }
        /// <summary>
        /// 重写后的标识。
        /// </summary>
        /// <returns>一个字符串。</returns>
        public abstract override string ToString();

        #region Equality members

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            var model = obj as AddressModel;
            if (model == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            return model.ToString() == ToString();
        }

        /// <summary>Serves as the default hash function. </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public static bool operator ==(AddressModel model1, AddressModel model2)
        {
            return Equals(model1, model2);
        }

        public static bool operator !=(AddressModel model1, AddressModel model2)
        {
            return !Equals(model1, model2);
        }

        #endregion Equality members
    }
}