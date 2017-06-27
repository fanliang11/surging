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
namespace Magnum.Algorithms.Implementations
{
	using System.Collections.Generic;


	/// <summary>
	/// Maintains an index of nodes so that regular ints can be used to execute algorithms
	/// against objects with int-compare speed vs. .Equals() speed
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class NodeTable<T>
	{
		readonly IDictionary<T, int> _nodes;
		int _count;

		public NodeTable()
		{
			_nodes = new Dictionary<T, int>();
		}

		public NodeTable(int capacity)
		{
			_nodes = new Dictionary<T, int>(capacity);
		}

		/// <summary>
		/// Returns the index for the specified key, which can be any type that supports
		/// equality comparison
		/// </summary>
		/// <param name="key">The key to retrieve</param>
		/// <returns>The index that uniquely relates to the specified key</returns>
		public int this[T key]
		{
			get
			{
				int value;
				if (_nodes.TryGetValue(key, out value))
					return value;

				value = ++_count;
				_nodes.Add(key, value);

				return value;
			}
		}
	}
}