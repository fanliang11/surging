using Surging.Core.CPlatform.Serialization;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Surging.Core.CPlatform.Convertibles.Implementation
{
    /// <summary>
    /// 一个默认的类型转换提供程序。
    /// </summary>
    public class DefaultTypeConvertibleProvider : ITypeConvertibleProvider
    {
        private readonly ISerializer<object> _serializer;

        public DefaultTypeConvertibleProvider(ISerializer<object> serializer)
        {
            _serializer = serializer;
        }

        #region Implementation of ITypeConvertibleProvider

        /// <summary>
        /// 获取类型转换器。
        /// </summary>
        /// <returns>类型转换器集合。</returns>
        public IEnumerable<TypeConvertDelegate> GetConverters()
        {
            yield return EnumTypeConvert;
            yield return SimpleTypeConvert;
            yield return GuidTypeConvert;
            yield return ComplexTypeConvert;
            
        }

        #endregion Implementation of ITypeConvertibleProvider

        #region Private Method

        private static object EnumTypeConvert(object instance, Type conversionType)
        {
            if (instance == null || !conversionType.GetTypeInfo().IsEnum)
                return null;
            return Enum.Parse(conversionType, instance.ToString());
        }

        private static object SimpleTypeConvert(object instance, Type conversionType)
        {
            if (instance is IConvertible && UtilityType.ConvertibleType.GetTypeInfo().IsAssignableFrom(conversionType))
                return Convert.ChangeType(instance, conversionType);
            return null;
        }

        private object ComplexTypeConvert(object instance, Type conversionType)
        {
            try
            {
                return _serializer.Deserialize(instance, conversionType);
            }
            catch
            {
                return null;
            }
        }

        private static object GuidTypeConvert(object instance, Type conversionType)
        {
            if (instance == null || conversionType != typeof(Guid))
                return null;
            Guid.TryParse(instance.ToString(), out Guid result);
            return result;
        }

        #endregion Private Method
    }
}