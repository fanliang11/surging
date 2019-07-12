using System;
using System.Collections.Generic;
using System.Linq;

namespace Surging.Core.SwaggerGen
{
    /// <summary>
    /// Defines the <see cref="SchemaIdManager" />
    /// </summary>
    public class SchemaIdManager
    {
        #region 字段

        /// <summary>
        /// Defines the _schemaIdMap
        /// </summary>
        private readonly IDictionary<Type, string> _schemaIdMap;

        /// <summary>
        /// Defines the _schemaIdSelector
        /// </summary>
        private readonly Func<Type, string> _schemaIdSelector;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaIdManager"/> class.
        /// </summary>
        /// <param name="schemaIdSelector">The schemaIdSelector<see cref="Func{Type, string}"/></param>
        public SchemaIdManager(Func<Type, string> schemaIdSelector)
        {
            _schemaIdSelector = schemaIdSelector;
            _schemaIdMap = new Dictionary<Type, string>();
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The IdFor
        /// </summary>
        /// <param name="type">The type<see cref="Type"/></param>
        /// <returns>The <see cref="string"/></returns>
        public string IdFor(Type type)
        {
            if (!_schemaIdMap.TryGetValue(type, out string schemaId))
            {
                schemaId = _schemaIdSelector(type);

                // Raise an exception if another type with same schemaId
                if (_schemaIdMap.Any(entry => entry.Value == schemaId))
                    throw new InvalidOperationException(string.Format(
                        "Conflicting schemaIds: Identical schemaIds detected for types {0} and {1}. " +
                        "See config settings - \"CustomSchemaIds\" for a workaround",
                        type.FullName, _schemaIdMap.First(entry => entry.Value == schemaId).Key));

                _schemaIdMap.Add(type, schemaId);
            }

            return schemaId;
        }

        #endregion 方法
    }
}