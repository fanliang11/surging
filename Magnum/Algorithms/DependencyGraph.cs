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
namespace Magnum.Algorithms
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Implementations;


	public class DependencyGraph<T>
	{
		readonly AdjacencyList<T, DependencyGraphNode<T>> _adjacencyList;

		public DependencyGraph()
		{
			_adjacencyList = new AdjacencyList<T, DependencyGraphNode<T>>(DefaultNodeFactory);
		}

		static DependencyGraphNode<T> DefaultNodeFactory(int index, T value)
		{
			return new DependencyGraphNode<T>(index, value);
		}

		public void Add(T source, T target)
		{
			_adjacencyList.AddEdge(source, target, 0);
		}

		public IEnumerable<T> GetItemsInDependencyOrder()
		{
			EnsureGraphIsAcyclic();

			var sort = new TopologicalSort<T, DependencyGraphNode<T>>(_adjacencyList.Clone());
			
			return sort.Result.Select(x => x.Value);
		}

		public IEnumerable<T> GetItemsInDependencyOrder(T source)
		{
			EnsureGraphIsAcyclic();

			var sort = new TopologicalSort<T, DependencyGraphNode<T>>(_adjacencyList.Clone(), source);
			
			return sort.Result.Select(x => x.Value);
		}

		void EnsureGraphIsAcyclic()
		{
			var tarjan = new Tarjan<T, DependencyGraphNode<T>>(_adjacencyList);

			if (tarjan.Result.Count == 0)
				return;

			StringBuilder message = new StringBuilder();
			foreach (var cycle in tarjan.Result)
			{
				message.Append("(");
				for (int i = 0; i < cycle.Count; i++)
				{
					if (i > 0)
						message.Append(",");

					message.Append(cycle[i].Value);
				}
				message.Append(")");
			}

			throw new InvalidOperationException("The dependency graph contains cycles: " + message);
		}
	}
}