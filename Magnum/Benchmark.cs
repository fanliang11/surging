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
namespace Magnum
{
	using System.Collections.Generic;


	/// <summary>
	/// A class that can be benchmarked by the console
	/// </summary>
	public interface Benchmark<T>
	{
		/// <summary>
		/// A list of iteration counts to execute with each benchmark
		/// </summary>
		IEnumerable<int> Iterations { get; }

		/// <summary>
		/// Any single-run operations that should be performed to prepare for the benchmark
		/// </summary>
		/// <param name="instance">The instance being tested</param>
		void WarmUp(T instance);

		/// <summary>
		/// Any post-run operations that should be performed to clean up
		/// </summary>
		/// <param name="instance"></param>
		void Shutdown(T instance);

		/// <summary>
		/// Run the operation being benchmarked the specified number of iterations
		/// </summary>
		/// <param name="instance">The instance being tested</param>
		/// <param name="iterationCount">The number of iterations to execute</param>
		void Run(T instance, int iterationCount);
	}
}