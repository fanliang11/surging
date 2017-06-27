// Copyright 2007-2008 The Apache Software Foundation.
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
namespace Magnum.Collections
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Reflection.Emit;
	using System.Threading;

	/// <summary>
	/// Converts an object of an anonymous type to a dictionary
	/// </summary>
	public class ObjectToDictionaryConverter
	{
		private readonly Dictionary<Type, Func<object, IDictionary<string, object>>> _cache;
		private readonly Dictionary<string, object> _default;
		private readonly ReaderWriterLockSlim _lock;

		public ObjectToDictionaryConverter()
		{
			_cache = new Dictionary<Type, Func<object, IDictionary<string, object>>>();
			_lock = new ReaderWriterLockSlim();
			_default = new Dictionary<string, object>();
		}

		public IDictionary<string, object> Convert(object dataObject)
		{
			if (dataObject == null)
				return _default;

			if (dataObject is IDictionary<string, object>)
				return (IDictionary<string, object>) dataObject;

			return GetObjectToDictionaryConverter(dataObject)(dataObject);
		}

		private Func<object, IDictionary<string, object>> GetObjectToDictionaryConverter(object item)
		{
			_lock.EnterUpgradeableReadLock();
			try
			{
				Func<object, IDictionary<string, object>> ft;
				if (!_cache.TryGetValue(item.GetType(), out ft))
				{
					_lock.EnterWriteLock();
					try
					{
						if (!_cache.TryGetValue(item.GetType(), out ft))
						{
							ft = CreateObjectToDictionaryConverter(item.GetType());
							_cache[item.GetType()] = ft;
						}
					}
					finally
					{
						_lock.ExitWriteLock();
					}
				}
				return ft;
			}
			finally
			{
				_lock.ExitUpgradeableReadLock();
			}
		}

		private static Func<object, IDictionary<string, object>> CreateObjectToDictionaryConverter(Type itemType)
		{
			Type dictType = typeof (Dictionary<string, object>);

			// setup dynamic method
			// Important: make itemType owner of the method to allow access to internal types
			var dm = new DynamicMethod(string.Empty, typeof (IDictionary<string, object>), new[] {typeof (object)}, itemType);
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
				{
					il.Emit(OpCodes.Box, property.PropertyType);
				}

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

			return (Func<object, IDictionary<string, object>>) dm.CreateDelegate(typeof (Func<object, IDictionary<string, object>>));
		}
	}
}