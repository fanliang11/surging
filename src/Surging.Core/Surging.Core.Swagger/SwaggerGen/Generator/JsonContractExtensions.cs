using Newtonsoft.Json.Serialization;

namespace Surging.Core.SwaggerGen
{
    /// <summary>
    /// Defines the <see cref="JsonContractExtensions" />
    /// </summary>
    internal static class JsonContractExtensions
    {
        #region 方法

        /// <summary>
        /// The IsSelfReferencingArrayOrDictionary
        /// </summary>
        /// <param name="jsonContract">The jsonContract<see cref="JsonContract"/></param>
        /// <returns>The <see cref="bool"/></returns>
        internal static bool IsSelfReferencingArrayOrDictionary(this JsonContract jsonContract)
        {
            if (jsonContract is JsonArrayContract arrayContract)
                return arrayContract.UnderlyingType == arrayContract.CollectionItemType;

            if (jsonContract is JsonDictionaryContract dictionaryContract)
                return dictionaryContract.UnderlyingType == dictionaryContract.DictionaryValueType;

            return false;
        }

        #endregion 方法
    }
}