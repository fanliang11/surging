using System.Text;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.ComponentModel.DataAnnotations.Schema;

namespace Repository.Dapper.Core.Mapper
{
    /// <summary>
    /// Automatically maps an entity to a table using a combination of reflection and naming conventions for keys.
    /// </summary>
    public class AutoClassMapper<T> : ClassMapper<T> where T : class
    {
        public AutoClassMapper()
        {
            Type type = typeof(T);
            var vTable = type.GetTypeInfo().GetCustomAttributes(typeof(TableAttribute), true).ToArray();
            var TableName = vTable.Length == 0 ? type.Name : (vTable[0] as TableAttribute).Name;
            Table(TableName);// Table(type.Name);
            if (vTable.Length > 0 && !string.IsNullOrWhiteSpace((vTable[0] as TableAttribute).Schema))
                Schema((vTable[0] as TableAttribute).Schema);

            var vNotKey = type.GetTypeInfo().GetCustomAttributes(typeof(NotKeyAttribute), true).ToArray();
            if (vNotKey.Length > 0)
                DefinedKey(false);
            AutoMap();
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class NotKeyAttribute : Attribute
    {
        public NotKeyAttribute()
        {
            NoDefinedKey = true;
        }

        public bool NoDefinedKey { get; set; }
    }
}