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
namespace Magnum.Graphing
{
	using System;
	using System.Collections.Generic;


	/// <summary>
	/// A provider of graph data can return vertices and edges
	/// </summary>
	[Serializable]
	public abstract class GraphData
	{
		readonly List<Edge> _edges;
		readonly List<Vertex> _vertices;

		protected GraphData(IEnumerable<Vertex> vertices, IEnumerable<Edge> edges)
		{
			_vertices = new List<Vertex>(vertices);
			_edges = new List<Edge>(edges);
		}

		public IEnumerable<Vertex> Vertices
		{
			get { return _vertices; }
		}

		public IEnumerable<Edge> Edges
		{
			get { return _edges; }
		}
	}
}