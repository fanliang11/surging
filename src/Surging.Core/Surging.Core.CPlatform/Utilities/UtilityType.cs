using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Utilities
{
    /// <summary>
    /// Defines the <see cref="UtilityType" />
    /// </summary>
    public static class UtilityType
    {
        #region 字段

        /// <summary>
        /// Defines the ConvertibleType
        /// </summary>
        public static Type ConvertibleType = typeof(IConvertible);

        /// <summary>
        /// Defines the JArrayType
        /// </summary>
        public static Type JArrayType = typeof(JArray);

        /// <summary>
        /// Defines the JObjectType
        /// </summary>
        public static Type JObjectType = typeof(JObject);

        /// <summary>
        /// Defines the ObjectType
        /// </summary>
        public static Type ObjectType = typeof(Object);

        #endregion 字段
    }
}