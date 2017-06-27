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
namespace Magnum.Serialization
{
	using System.IO;

	/// <summary>
	/// Serializers convert objects to some type of string output
	/// </summary>
	public interface Serializer
	{
		/// <summary>
		/// Serialize an object to the specified TextWriter
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <param name="writer"></param>
		void Serialize<T>(T obj, TextWriter writer);

		/// <summary>
		/// Serialize an object and return the string representing the object data
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <returns></returns>
		string Serialize<T>(T obj);

		/// <summary>
		/// Deserialize an object from a string
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="text"></param>
		/// <returns></returns>
		T Deserialize<T>(string text);

		/// <summary>
		/// Deserialize an object from a TextReader
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="reader"></param>
		/// <returns></returns>
		T Deserialize<T>(TextReader reader);
	}
}