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


	public class AdjacencyList<T, TNode>
		where TNode : Node<T>
	{
		readonly Func<int, T, TNode> _nodeFactory;
		readonly NodeList<T, TNode> _nodeList;
		IDictionary<TNode, HashSet<Edge<T, TNode>>> _adjacencies;


		public AdjacencyList(Func<int, T, TNode> nodeFactory)
		{
			_nodeFactory = nodeFactory;
			_nodeList = new NodeList<T, TNode>(nodeFactory);
			_adjacencies = new Dictionary<TNode, HashSet<Edge<T, TNode>>>();
		}

		public ICollection<TNode> SourceNodes
		{
			get { return _adjacencies.Keys; }
		}

		public HashSet<Edge<T, TNode>> GetEdges(TNode index)
		{
			HashSet<Edge<T, TNode>> edges;
			if (_adjacencies.TryGetValue(index, out edges))
				return edges;

			return new HashSet<Edge<T, TNode>>();
		}

		public HashSet<Edge<T, TNode>> GetEdges(T index)
		{
			return GetEdges(_nodeList[index]);
		}

		public void AddEdge(T source, T target, int weight)
		{
			TNode sourceNode = _nodeList[source];
			TNode targetNode = _nodeList[target];

			AddEdge(sourceNode, targetNode, weight);
		}

		public void AddEdge(TNode source, TNode target, int weight)
		{
			HashSet<Edge<T, TNode>> edges;
			if (!_adjacencies.TryGetValue(source, out edges))
			{
				edges = new HashSet<Edge<T, TNode>>();
				_adjacencies.Add(source, edges);
			}

			edges.Add(new Edge<T, TNode>(source, target, weight));
		}

		public void ReverseEdge(Edge<T, TNode> edge)
		{
			HashSet<Edge<T, TNode>> edges;
			if (_adjacencies.TryGetValue(edge.Source, out edges))
				edges.Remove(edge);

			AddEdge(edge.Target, edge.Source, edge.Weight);
		}

		public void ReverseList()
		{
			_adjacencies = Reverse()._adjacencies;
		}

		public AdjacencyList<T, TResultNode> Transform<TResultNode>(Func<int, T, TResultNode> nodeFactory)
			where TResultNode : Node<T>
		{
			var result = new AdjacencyList<T, TResultNode>(nodeFactory);

			foreach (var adjacency in _adjacencies.Values)
			{
				foreach (var edge in adjacency)
					result.AddEdge(edge.Source.Value, edge.Target.Value, edge.Weight);
			}

			return result;
		}

		public AdjacencyList<T, TNode> Clone()
		{
			return Transform(_nodeFactory);
		}

		public AdjacencyList<T, TNode> Reverse()
		{
			var result = new AdjacencyList<T, TNode>(_nodeFactory);
			foreach (var adjacency in _adjacencies.Values)
			{
				foreach (var edge in adjacency)
					result.AddEdge(edge.Target.Value, edge.Source.Value, edge.Weight);
			}

			return result;
		}

		public TNode GetNode(T key)
		{
			return _nodeList[key];
		}
	}
}