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
namespace Magnum.Serialization
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using Extensions;
	using Reflection;
	using TypeSerializers;


	/// <summary>
	///   Scans the assembly for implementations of the TypeSerializer interface for 
	///   the built-in types
	/// </summary>
	public class TypeSerializerLoader
	{
		public IDictionary<Type, TypeSerializer> LoadBuiltInTypeSerializers()
		{
			Dictionary<Type, TypeSerializer> serializers = Assembly.GetExecutingAssembly().GetTypes()
				.Where(x => x.Namespace == typeof(StringSerializer).Namespace)
				.Where(x => x.ImplementsGeneric(typeof(TypeSerializer<>)))
				.Where(x => !x.ContainsGenericParameters)
				.Select(x => new
					{
						Type = x,
						SerializedType = x.GetGenericTypeDeclarations(typeof(TypeSerializer<>)).First()
					})
				.Select(x => new
					{
						x.SerializedType,
						Serializer = FastActivator.Create(x.Type) as TypeSerializer
					})
				.ToDictionary(x => x.SerializedType, x => x.Serializer);

			return serializers;
		}
	}
}