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


	public struct Edge<T, TNode> :
		IComparable<Edge<T, TNode>>
		where TNode : Node<T>
	{
		public readonly TNode Source;
		public readonly TNode Target;
		public readonly int Weight;

		public Edge(TNode source, TNode target, int weight)
		{
			Source = source;
			Target = target;
			Weight = weight;
		}

		public int CompareTo(Edge<T, TNode> other)
		{
			return Weight - other.Weight;
		}
	}
}