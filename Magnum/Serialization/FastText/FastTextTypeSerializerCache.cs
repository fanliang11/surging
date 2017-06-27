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
namespace Magnum.Serialization.FastText
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using Collections;
	using Extensions;
	using Reflection;
	using TypeSerializers;


	public class FastTextTypeSerializerCache :
		TypeSerializerCache
	{
		readonly PropertyTypeSerializerCache _propertyTypeSerializerCache;
		readonly Cache<Type, FastTextTypeSerializer> _serializers;

		public FastTextTypeSerializerCache(TypeSerializerCache typeSerializerCache)
		{
			_propertyTypeSerializerCache = new FastTextPropertyTypeSerializerCache(this);

			_serializers = new Cache<Type, FastTextTypeSerializer>(CreateSerializerFor);

			typeSerializerCache.Each((type, serializer) => _serializers.Add(type, CreateSerializerFor(type, serializer)));

			_serializers[typeof(string)] = new FastTextTypeSerializer<string>(new FastTextStringSerializer());
			_serializers[typeof(Type)] = new FastTextTypeSerializer<Type>(new FastTextSystemTypeSerializer());
		}

		public FastTextTypeSerializer this[Type type]
		{
			get { return _serializers[type]; }
		}

		public TypeSerializer<T> GetTypeSerializer<T>()
		{
			return _serializers[typeof(T)] as TypeSerializer<T>;
		}

		public void Each(Action<Type, TypeSerializer> action)
		{
			_serializers.Each((type, serializer) => action(type, serializer as TypeSerializer));
		}

		static FastTextTypeSerializer CreateSerializerFor(Type type, TypeSerializer serializer)
		{
			return
				(FastTextTypeSerializer)
				FastActivator.Create(typeof(FastTextTypeSerializer<>), new[] {type}, new object[] {serializer});
		}

		FastTextTypeSerializer CreateSerializerFor(Type type)
		{
			Type underlyingType = Nullable.GetUnderlyingType(type);
			if (underlyingType != null)
				return CreateNullableSerializer(type, underlyingType);

			if (type.IsEnum)
				return CreateEnumSerializerFor(type);

			if (typeof(IEnumerable).IsAssignableFrom(type))
				return CreateEnumerableSerializer(type);

			if (type.IsInterface)
				return CreateObjectSerializerForInterface(type);

			return CreateObjectSerializerFor(type);
		}

		FastTextTypeSerializer CreateObjectSerializerForInterface(Type type)
		{
			Type proxyType = InterfaceImplementationBuilder.GetProxyFor(type);

			return CreateObjectSerializerFor(proxyType);
		}

		FastTextTypeSerializer CreateObjectSerializerFor(Type type)
		{
			var serializer =
				(TypeSerializer)
				FastActivator.Create(typeof(FastTextObjectSerializer<>), new[] {type}, new object[] {_propertyTypeSerializerCache});
			return CreateSerializerFor(type, serializer);
		}

		FastTextTypeSerializer CreateNullableSerializer(Type type, Type underlyingType)
		{
			FastTextTypeSerializer underlyingTypeSerializer = this[underlyingType];

			var serializer =
				(TypeSerializer)
				FastActivator.Create(typeof(NullableSerializer<>), new[] {underlyingType}, new object[] {underlyingTypeSerializer});

			return CreateSerializerFor(type, serializer);
		}

		static FastTextTypeSerializer CreateEnumSerializerFor(Type type)
		{
			return CreateSerializerFor(type, CreateGenericSerializer(typeof(EnumSerializer<>), type));
		}

		static TypeSerializer CreateGenericSerializer(Type genericType, Type type)
		{
			return (TypeSerializer)FastActivator.Create(genericType.MakeGenericType(type));
		}

		FastTextTypeSerializer CreateArraySerializer(Type type, Type elementType)
		{
			FastTextTypeSerializer elementSerializer = this[elementType];

			object serializer = FastActivator.Create(typeof(FastTextArraySerializer<>), new[] {elementType},
			                                         new object[] {elementSerializer});


			return CreateSerializerFor(type, (TypeSerializer)serializer);
		}

		FastTextTypeSerializer CreateListSerializer(Type type, Type elementType)
		{
			FastTextTypeSerializer elementSerializer = this[elementType];

			Type listType = type.ImplementsGeneric(typeof(List<>))
			                	? typeof(FastTextListSerializer<>)
			                	: typeof(FastTextIListSerializer<>);

			object serializer = FastActivator.Create(listType, new[] {elementType},
			                                         new object[] {elementSerializer});


			return CreateSerializerFor(type, (TypeSerializer)serializer);
		}

		FastTextTypeSerializer CreateDictionarySerializer(Type type, Type keyType, Type elementType)
		{
			FastTextTypeSerializer keySerializer = this[keyType];
			FastTextTypeSerializer elementSerializer = this[elementType];

			Type dictionaryType = type.ImplementsGeneric(typeof(Dictionary<,>))
			                      	? typeof(FastTextDictionarySerializer<,>)
			                      	: typeof(FastTextIDictionarySerializer<,>);

			object serializer = FastActivator.Create(dictionaryType, new[] {keyType, elementType},
			                                         new object[] {keySerializer, elementSerializer});


			return CreateSerializerFor(type, (TypeSerializer)serializer);
		}

		FastTextTypeSerializer CreateEnumerableSerializer(Type type)
		{
			if (type.IsArray)
				return CreateArraySerializer(type, type.GetElementType());

			Type[] genericArguments = type.GetDeclaredGenericArguments().ToArray();
			if (genericArguments == null || genericArguments.Length == 0)
			{
				Type elementType = type.IsArray ? type.GetElementType() : typeof(object);

				return CreateArraySerializer(type, elementType);
			}

			if (type.ImplementsGeneric(typeof(IDictionary<,>)))
				return CreateDictionarySerializer(type, genericArguments[0], genericArguments[1]);

			if (type.ImplementsGeneric(typeof(IList<>)) || type.ImplementsGeneric(typeof(IEnumerable<>)))
				return CreateListSerializer(type, genericArguments[0]);

			throw new InvalidOperationException("The type of enumeration is not supported: " + type.FullName);
		}
	}
}