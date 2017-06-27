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
	public class Node<T>
	{
		public readonly T Value;
		readonly int _index;

		public Node(int index, T value)
		{
			_index = index;
			Value = value;
		}

		public int CompareTo(DependencyGraphNode<T> other)
		{
			return !Equals(other) ? 0 : -1;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;

			return false;
		}

		public override int GetHashCode()
		{
			return _index;
		}
	}
}