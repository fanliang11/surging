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


	public class DependencyGraphNode<T> :
		Node<T>,
		TopologicalSortNodeProperties,
		TarjanNodeProperties,
		IComparable<DependencyGraphNode<T>>
	{
		public int Index;
		public int LowLink;
		public bool Visited;

		public DependencyGraphNode(int index, T value)
			: base(index, value)
		{
			Visited = false;
			LowLink = -1;
			Index = -1;
		}


		int TarjanNodeProperties.Index
		{
			get { return Index; }
			set { Index = value; }
		}

		int TarjanNodeProperties.LowLink
		{
			get { return LowLink; }
			set { LowLink = value; }
		}

		bool TopologicalSortNodeProperties.Visited
		{
			get { return Visited; }
			set { Visited = value; }
		}
	}
}