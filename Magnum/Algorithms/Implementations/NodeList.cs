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
	using System;
	using System.Collections.Generic;


	public class NodeList<T> :
		NodeList<T, Node<T>>
	{
		public NodeList()
			: base(DefaultNodeFactory)
		{
		}

		public NodeList(int capacity)
			: base(DefaultNodeFactory, capacity)
		{
		}

		static Node<T> DefaultNodeFactory(int index, T value)
		{
			return new Node<T>(index, value);
		}
	}

	/// <summary>
	/// Maintains a list of nodes for a given set of instances of T
	/// </summary>
	/// <typeparam name="T">The type encapsulated in the node</typeparam>
	/// <typeparam name="TNode">The type of node contained in the list</typeparam>
	public class NodeList<T, TNode>
		where TNode : Node<T>
	{
		readonly Func<int, T, TNode> _nodeFactory;
		readonly NodeTable<T> _nodeTable;
		readonly IList<TNode> _nodes;

		public NodeList(Func<int, T, TNode> nodeFactory)
		{
			_nodeFactory = nodeFactory;
			_nodes = new List<TNode>();
			_nodeTable = new NodeTable<T>();
		}

		public NodeList(Func<int, T, TNode> nodeFactory, int capacity)
		{
			_nodeFactory = nodeFactory;
			_nodes = new List<TNode>(capacity);
			_nodeTable = new NodeTable<T>(capacity);
		}

		/// <summary>
		/// Retrieves the node for the given key
		/// </summary>
		/// <param name="key">The key</param>
		/// <returns>The unique node that relates to the specified key</returns>
		public TNode this[T key]
		{
			get { return _nodes[Index(key) - 1]; }
		}

		/// <summary>
		/// Retrieve the index for a given key
		/// </summary>
		/// <param name="key">The key</param>
		/// <returns>The index</returns>
		public int Index(T key)
		{
			int index = _nodeTable[key];

			if (index <= _nodes.Count)
				return index;

			TNode node = _nodeFactory(index, key);
			_nodes.Add(node);

			return index;
		}
	}
}