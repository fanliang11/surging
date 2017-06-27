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
namespace Magnum.Serialization.TypeSerializers
{
	using System;
	using System.Linq;
	using System.Reflection;


	public class SystemTypeSerializer
		: TypeSerializer<Type>
	{
		public TypeReader<Type> GetReader()
		{
			return value =>
				{
					Type type = Type.GetType(value, false);
					if (type == null)
						type = GetTypeWithoutVersionString(value);

					if (type == null)
					{
						throw new TypeSerializerException("The type [" + value
						                                  + "] is not available and the property could not be deserialized");
					}

					return type;
				};
		}

		public TypeWriter<Type> GetWriter()
		{
			return (value, output) =>
				{
					if (value == null)
						return;

					output(value.AssemblyQualifiedName);
				};
		}

		static Type GetTypeWithoutVersionString(string ns)
		{
			int assemblyIndex = ns.IndexOf(",");
			if (assemblyIndex <= 0)
				return null;

			var assemblyName = new AssemblyName(ns.Substring(assemblyIndex + 1));

			Assembly assembly = AppDomain.CurrentDomain.GetAssemblies()
				.Where(x => x.GetName().Name == assemblyName.Name
				            && x.GetName().GetPublicKeyToken().SequenceEqual(assemblyName.GetPublicKeyToken()))
				.FirstOrDefault();

			if (assembly == null)
				return null;

			string typeName = ns.Substring(0, assemblyIndex);

			return assembly.GetType(typeName, false);
		}
	}
}