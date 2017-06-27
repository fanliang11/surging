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


	public class Tarjan<T, TNode>
		where TNode : Node<T>, TarjanNodeProperties
	{
		readonly AdjacencyList<T, TNode> _list;
		readonly IList<IList<TNode>> _result;
		readonly Stack<TNode> _stack;
		int _index;

		public Tarjan(AdjacencyList<T, TNode> list)
		{
			_list = list;
			_index = 0;
			_result = new List<IList<TNode>>();
			_stack = new Stack<TNode>();

			foreach (TNode node in _list.SourceNodes)
			{
				if (node.Index != -1)
					continue;

				Compute(node);
			}
		}

		public IList<IList<TNode>> Result
		{
			get { return _result; }
		}

		void Compute(TNode v)
		{
			v.Index = _index;
			v.LowLink = _index;
			_index++;

			_stack.Push(v);

			foreach (var edge in _list.GetEdges(v))
			{
				TNode n = edge.Target;
				if (n.Index == -1)
				{
					Compute(n);
					v.LowLink = Math.Min(v.LowLink, n.LowLink);
				}
				else if (_stack.Contains(n))
					v.LowLink = Math.Min(v.LowLink, n.Index);
			}

			if (v.LowLink == v.Index)
			{
				TNode n;
				IList<TNode> component = new List<TNode>();
				do
				{
					n = _stack.Pop();
					component.Add(n);
				}
				while (!v.Equals(n));

				if (component.Count != 1 || !v.Equals(component[0]))
					_result.Add(component);
			}
		}
	}
}