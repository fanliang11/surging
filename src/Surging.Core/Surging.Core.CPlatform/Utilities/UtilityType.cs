using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Utilities
{
    public static class UtilityType
    {
        public static Type JObjectType = typeof(JObject);

        public static Type JArrayType = typeof(JArray);

        public static Type ObjectType = typeof(Object);

        public static Type ConvertibleType = typeof(IConvertible);
    }
}
