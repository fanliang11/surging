using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Surging.Core.CPlatform.Utilities
{
    public static class UtilityType
    {
        public static Type JObjectType = typeof(JObject);

        public static Type JsonEl= typeof(JsonElement);

        public static Type JArrayType = typeof(JArray);

        public static Type JsonElementType = typeof(JsonElement);

        public static Type ObjectType = typeof(Object);

        public static Type ConvertibleType = typeof(IConvertible);
    }
}
