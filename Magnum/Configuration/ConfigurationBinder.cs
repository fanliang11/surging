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
namespace Magnum.Configuration
{
    using System;
    using System.Collections.Generic;


	/// <summary>
	/// Creates configuration/settings objects, applying configuration values
	/// to the properties as they match the configuration keys in the store
	/// </summary>
	public interface ConfigurationBinder
	{
		/// <summary>
		/// Creates an instance of the specified type and sets the properties
		/// to the configuration settings that match
		/// </summary>
		/// <typeparam name="T">The configuration object type</typeparam>
		/// <returns>An initialize configuration object</returns>
		T Bind<T>();

        /// <summary>
        /// Creates an instance of the specified type and sets the properties
        /// to the configuration settings that match
        /// </summary>
        /// <typeparam name="T">The configuration object type</typeparam>
        /// <returns>An initialize configuration object</returns>
        object Bind(Type typeToBindTo);

		/// <summary>
		/// Returns a single configuration value
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		object GetValue(string key);

		/// <summary>
		/// Returns a single configuration value of type T
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <returns></returns>
		T GetValue<T>(string key);

		/// <summary>
		/// Returns a single configuration as a string
		/// </summary>
		/// <param name="key"></param>
		/// <returns>The string representation of the value, or null</returns>
		string GetValueAsString(string key);

		/// <summary>
		/// Return a dictionary of all key/value pairs that exist
		/// </summary>
		/// <returns></returns>
		IDictionary<string, object> GetAll();
	}
}