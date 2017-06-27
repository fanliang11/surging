// Copyright 2007-2010 The Apache Software Foundation.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Magnum.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using Caching;


    public class AnonymousObjectDictionaryConverter
    {
        readonly Cache<Type, Func<object, IDictionary<string, object>>> _cache;
        readonly Dictionary<string, object> _default;

        public AnonymousObjectDictionaryConverter()
        {
            _cache = new ConcurrentCache<Type, Func<object, IDictionary<string, object>>>(CreateObjectToDictionaryConverter);
            _default = new Dictionary<string, object>();
        }

        public IDictionary<string, object> Convert(object dataObject)
        {
            if (dataObject == null)
                return _default;

            if (dataObject is IDictionary<string, object>)
                return (IDictionary<string, object>)dataObject;

            return _cache[dataObject.GetType()](dataObject);
        }

        static Func<object, IDictionary<string, object>> CreateObjectToDictionaryConverter(Type itemType)
        {
            Type dictType = typeof(Dictionary<string, object>);

            // setup dynamic method
            // Important: make itemType owner of the method to allow access to internal types
            var dm = new DynamicMethod(string.Empty, typeof(IDictionary<string, object>), new[] {typeof(object)},
                                       itemType);
            ILGenerator il = dm.GetILGenerator();

            // Dictionary.Add(object key, object value)
            MethodInfo addMethod = dictType.GetMethod("Add");

            // create the Dictionary and store it in a local variable
            il.DeclareLocal(dictType);
            il.Emit(OpCodes.Newobj, dictType.GetConstructor(Type.EmptyTypes));
            il.Emit(OpCodes.Stloc_0);

            BindingFlags attributes = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
            foreach (PropertyInfo property in itemType.GetProperties(attributes).Where(info => info.CanRead))
            {
                // load Dictionary (prepare for call later)
                il.Emit(OpCodes.Ldloc_0);
                // load key, i.e. name of the property
                il.Emit(OpCodes.Ldstr, property.Name);

                // load value of property to stack
                il.Emit(OpCodes.Ldarg_0);
                il.EmitCall(OpCodes.Callvirt, property.GetGetMethod(), null);
                // perform boxing if necessary
                if (property.PropertyType.IsValueType)
                    il.Emit(OpCodes.Box, property.PropertyType);

                // stack at this point
                // 1. string or null (value)
                // 2. string (key)
                // 3. dictionary

                // ready to call dict.Add(key, value)
                il.EmitCall(OpCodes.Callvirt, addMethod, null);
            }
            // finally load Dictionary and return
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);

            return
                (Func<object, IDictionary<string, object>>)
                dm.CreateDelegate(typeof(Func<object, IDictionary<string, object>>));
        }
    }
}