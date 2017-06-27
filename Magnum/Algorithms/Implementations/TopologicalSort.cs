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
	using System.Linq;



	public class TopologicalSort<T, TNode>
		where TNode : Node<T>, TopologicalSortNodeProperties
	{
		readonly AdjacencyList<T, TNode> _list;
		readonly IList<TNode> _results;
		readonly IEnumerable<TNode> _sourceNodes;

		public TopologicalSort(AdjacencyList<T, TNode> list)
		{
			_list = list;
			_results = new List<TNode>();
			_sourceNodes = _list.SourceNodes;

			Sort();
		}

		public TopologicalSort(AdjacencyList<T, TNode> list, T source)
		{
			_list = list;
			_results = new List<TNode>();

			TNode sourceNode = list.GetNode(source);
			_sourceNodes = Enumerable.Repeat(sourceNode, 1);

			Sort();
		}

		public IEnumerable<TNode> Result
		{
			get { return _results; }
		}

		void Sort()
		{
			foreach (TNode node in _sourceNodes)
			{
				if (!node.Visited)
					Sort(node);
			}
		}

		void Sort(TNode node)
		{
			node.Visited = true;
			foreach (var edge in _list.GetEdges(node))
			{
				if (!edge.Target.Visited)
					Sort(edge.Target);
			}

			_results.Add(node);
		}
	}
}